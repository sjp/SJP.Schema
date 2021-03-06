﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Query;
using SJP.Schematic.MySql.QueryResult;

namespace SJP.Schematic.MySql.Comments
{
    /// <summary>
    /// A table comment provider for MySQL databases.
    /// </summary>
    /// <seealso cref="IRelationalDatabaseTableCommentProvider" />
    public class MySqlTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTableCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the given database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
        public MySqlTableCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection factory to query the database.
        /// </summary>
        /// <value>A connection factory.</value>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Retrieves comments for all database tables.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database table comments, where available.</returns>
        public async IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<GetAllTableCommentsQueryResult>(
                AllTableCommentsQuery,
                new GetAllTableCommentsQuery { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            ).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(static row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var tableName = QualifyTableName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));
                    var comments = g.ToList();

                    var tableComment = GetFirstCommentByType(comments, Constants.Table);
                    var primaryKeyComment = Option<string>.None;
                    var columnComments = GetCommentLookupByType(comments, Constants.Column);
                    var checkComments = Empty.CommentLookup;
                    var foreignKeyComments = Empty.CommentLookup;
                    var uniqueKeyComments = Empty.CommentLookup;
                    var indexComments = GetCommentLookupByType(comments, Constants.Index);
                    var triggerComments = Empty.CommentLookup;

                    return new RelationalDatabaseTableComments(
                        tableName,
                        tableComment,
                        primaryKeyComment,
                        columnComments,
                        checkComments,
                        uniqueKeyComments,
                        foreignKeyComments,
                        indexComments,
                        triggerComments
                    );
                });

            foreach (var comment in comments)
                yield return comment;
        }

        /// <summary>
        /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="tableName">A table name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = QualifyTableName(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<GetTableNameQueryResult>(
                TableNameQuery,
                new GetTableNameQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.TableName));
        }

        /// <summary>
        /// A SQL query definition that resolves a table name for the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableNameQuery => TableNameQuerySql;

        private static readonly string TableNameQuerySql = @$"
select table_schema as `{ nameof(GetTableNameQueryResult.SchemaName) }`, table_name as `{ nameof(GetTableNameQueryResult.TableName) }`
from information_schema.tables
where table_schema = @{ nameof(GetTableNameQuery.SchemaName) } and table_name = @{ nameof(GetTableNameQuery.TableName) }
limit 1";

        /// <summary>
        /// Retrieves comments for a database table, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return LoadTableComments(candidateTableName, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's comments.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for a table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IRelationalDatabaseTableComments> LoadTableComments(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IRelationalDatabaseTableComments> LoadTableCommentsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var commentsData = await Connection.QueryAsync<GetTableCommentsQueryResult>(
                TableCommentsQuery,
                new GetTableCommentsQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var tableComment = GetFirstCommentByType(commentsData, Constants.Table);
            var primaryKeyComment = Option<string>.None;

            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);
            var checkComments = Empty.CommentLookup;
            var foreignKeyComments = Empty.CommentLookup;
            var uniqueKeyComments = Empty.CommentLookup;
            var indexComments = GetCommentLookupByType(commentsData, Constants.Index);
            var triggerComments = Empty.CommentLookup;

            return new RelationalDatabaseTableComments(
                tableName,
                tableComment,
                primaryKeyComment,
                columnComments,
                checkComments,
                uniqueKeyComments,
                foreignKeyComments,
                indexComments,
                triggerComments
            );
        }

        /// <summary>
        /// A SQL query definition which retrieves all comment information for all tables.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllTableCommentsQuery => AllTableCommentsQuerySql;

        private static readonly string AllTableCommentsQuerySql = @$"
select wrapped.* from (
-- table
select
    TABLE_SCHEMA as `{ nameof(GetAllTableCommentsQueryResult.SchemaName) }`,
    TABLE_NAME as `{ nameof(GetAllTableCommentsQueryResult.TableName) }`,
    'TABLE' as `{ nameof(GetAllTableCommentsQueryResult.ObjectType) }`,
    TABLE_NAME as `{ nameof(GetAllTableCommentsQueryResult.ObjectName) }`,
    TABLE_COMMENT as `{ nameof(GetAllTableCommentsQueryResult.Comment) }`
from INFORMATION_SCHEMA.TABLES
where TABLE_SCHEMA = @{ nameof(GetAllTableCommentsQuery.SchemaName) }

union

-- columns
select
    c.TABLE_SCHEMA as `{ nameof(GetAllTableCommentsQueryResult.SchemaName) }`,
    c.TABLE_NAME as `{ nameof(GetAllTableCommentsQueryResult.TableName) }`,
    'COLUMN' as `{ nameof(GetAllTableCommentsQueryResult.ObjectType) }`,
    c.COLUMN_NAME as `{ nameof(GetAllTableCommentsQueryResult.ObjectName) }`,
    c.COLUMN_COMMENT as `{ nameof(GetAllTableCommentsQueryResult.Comment) }`
from INFORMATION_SCHEMA.COLUMNS c
inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_NAME = t.TABLE_NAME
where c.TABLE_SCHEMA = @{ nameof(GetAllTableCommentsQuery.SchemaName) }

union

-- indexes
select
    s.TABLE_SCHEMA as `{ nameof(GetAllTableCommentsQueryResult.SchemaName) }`,
    s.TABLE_NAME as `{ nameof(GetAllTableCommentsQueryResult.TableName) }`,
    'INDEX' as `{ nameof(GetAllTableCommentsQueryResult.ObjectType) }`,
    s.INDEX_NAME as `{ nameof(GetAllTableCommentsQueryResult.ObjectName) }`,
    s.INDEX_COMMENT as `{ nameof(GetAllTableCommentsQueryResult.Comment) }`
from INFORMATION_SCHEMA.STATISTICS s
inner join INFORMATION_SCHEMA.TABLES t on s.TABLE_SCHEMA = t.TABLE_SCHEMA and s.TABLE_NAME = t.TABLE_NAME
where s.TABLE_SCHEMA = @{ nameof(GetAllTableCommentsQuery.SchemaName) }
) wrapped order by wrapped.{ nameof(GetAllTableCommentsQueryResult.SchemaName) }, wrapped.{ nameof(GetAllTableCommentsQueryResult.TableName) }
";

        /// <summary>
        /// A SQL query definition which retrieves all comment information for a particular table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableCommentsQuery => TableCommentsQuerySql;

        private static readonly string TableCommentsQuerySql = @$"
-- table
select
    'TABLE' as `{ nameof(GetTableCommentsQueryResult.ObjectType) }`,
    TABLE_NAME as `{ nameof(GetTableCommentsQueryResult.ObjectName) }`,
    TABLE_COMMENT as `{ nameof(GetTableCommentsQueryResult.Comment) }`
from INFORMATION_SCHEMA.TABLES
where TABLE_SCHEMA = @{ nameof(GetTableCommentsQuery.SchemaName) } and TABLE_NAME = @{ nameof(GetTableCommentsQuery.TableName) }

union

-- columns
select
    'COLUMN' as `{ nameof(GetTableCommentsQueryResult.ObjectType) }`,
    c.COLUMN_NAME as `{ nameof(GetTableCommentsQueryResult.ObjectName) }`,
    c.COLUMN_COMMENT as `{ nameof(GetTableCommentsQueryResult.Comment) }`
from INFORMATION_SCHEMA.COLUMNS c
inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_NAME = t.TABLE_NAME
where c.TABLE_SCHEMA = @{ nameof(GetTableCommentsQuery.SchemaName) } and c.TABLE_NAME = @{ nameof(GetTableCommentsQuery.TableName) }

union

-- indexes
select
    'INDEX' as `{ nameof(GetTableCommentsQueryResult.ObjectType) }`,
    s.INDEX_NAME as `{ nameof(GetTableCommentsQueryResult.ObjectName) }`,
    s.INDEX_COMMENT as `{ nameof(GetTableCommentsQueryResult.Comment) }`
from INFORMATION_SCHEMA.STATISTICS s
inner join INFORMATION_SCHEMA.TABLES t on s.TABLE_SCHEMA = t.TABLE_SCHEMA and s.TABLE_NAME = t.TABLE_NAME
where s.TABLE_SCHEMA = @{ nameof(GetTableCommentsQuery.SchemaName) } and s.TABLE_NAME = @{ nameof(GetTableCommentsQuery.TableName) }
";

        private static Option<string> GetFirstCommentByType(IEnumerable<GetAllTableCommentsQueryResult> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static Option<string> GetFirstCommentByType(IEnumerable<GetTableCommentsQueryResult> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<GetAllTableCommentsQueryResult> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(static c => new KeyValuePair<Identifier, Option<string>>(
                    Identifier.CreateQualifiedIdentifier(c.ObjectName),
                    !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None
                ))
                .ToReadOnlyDictionary(IdentifierComparer.Ordinal);
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<GetTableCommentsQueryResult> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(static c => new KeyValuePair<Identifier, Option<string>>(
                    Identifier.CreateQualifiedIdentifier(c.ObjectName),
                    !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None
                ))
                .ToReadOnlyDictionary(IdentifierComparer.Ordinal);
        }

        /// <summary>
        /// Qualifies the name of a table, using known identifier defaults.
        /// </summary>
        /// <param name="tableName">A table name to qualify.</param>
        /// <returns>A table name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected Identifier QualifyTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var schema = tableName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
        }

        private static class Constants
        {
            public const string Table = "TABLE";

            public const string Column = "COLUMN";

            public const string Index = "INDEX";
        }
    }
}
