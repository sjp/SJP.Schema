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
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer.Comments
{
    /// <summary>
    /// A database sequence comment provider for SQL Server.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceCommentProvider" />
    public class SqlServerSequenceCommentProvider : IDatabaseSequenceCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerSequenceCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
        public SqlServerSequenceCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection factory.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Retrieves the extended property name used to store comments on an object.
        /// </summary>
        /// <value>The comment property name.</value>
        protected virtual string CommentProperty { get; } = "MS_Description";

        /// <summary>
        /// Retrieves comments for all database sequences.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database sequence comments.</returns>
        public async IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllSequenceCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var sequenceComments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var sequenceName = QualifySequenceName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));
                    var comments = g.ToList();

                    var sequenceComment = GetFirstCommentByType(comments, Constants.Sequence);
                    return new DatabaseSequenceComments(sequenceName, sequenceComment);
                });

            foreach (var sequenceComment in sequenceComments)
                yield return sequenceComment;
        }

        /// <summary>
        /// Gets the resolved name of the sequence. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="sequenceName">A sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = QualifySequenceName(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// Gets a query that resolves the name of a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private static readonly string SequenceNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(QualifiedName.SchemaName) }], name as [{ nameof(QualifiedName.ObjectName) }]
from sys.sequences
where schema_id = schema_id(@SchemaName) and name = @SequenceName and is_ms_shipped = 0";

        /// <summary>
        /// Retrieves comments for a particular database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of a database sequence.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OptionAsync{A}" /> instance which holds the value of the sequence's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceComments(candidateSequenceName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a particular database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of a database sequence.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OptionAsync{A}" /> instance which holds the value of the sequence's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSequenceComments> LoadSequenceComments(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
                .MapAsync(name => LoadSequenceCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseSequenceComments> LoadSequenceCommentsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var commentsData = await Connection.QueryAsync<CommentsData>(
                SequenceCommentsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var sequenceComment = GetFirstCommentByType(commentsData, Constants.Sequence);

            return new DatabaseSequenceComments(sequenceName, sequenceComment);
        }

        /// <summary>
        /// Gets a query that retrieves comment information on all sequences.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllSequenceCommentsQuery => AllSequenceCommentsQuerySql;

        private static readonly string AllSequenceCommentsQuerySql = @$"
select
    SCHEMA_NAME(s.schema_id) as [{ nameof(CommentsData.SchemaName) }],
    s.name as [{ nameof(CommentsData.TableName) }],
    'SEQUENCE' as [{ nameof(CommentsData.ObjectType) }],
    s.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.sequences s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.is_ms_shipped = 0
order by SCHEMA_NAME(s.schema_id), s.name
";

        /// <summary>
        /// Gets a query that retrieves comment information on a single comment.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceCommentsQuery => SequenceCommentsQuerySql;

        private static readonly string SequenceCommentsQuerySql = @$"
select
    'SEQUENCE' as [{ nameof(CommentsData.ObjectType) }],
    s.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.sequences s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.schema_id = SCHEMA_ID(@SchemaName) and s.name = @SequenceName and s.is_ms_shipped = 0
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentsData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => c.ObjectType == objectType)
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        /// <summary>
        /// Qualifies the name of the sequence.
        /// </summary>
        /// <param name="sequenceName">A view name.</param>
        /// <returns>A sequence name is at least as qualified as the given sequence name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected Identifier QualifySequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
        }

        private static class Constants
        {
            public const string Sequence = "SEQUENCE";
        }
    }
}
