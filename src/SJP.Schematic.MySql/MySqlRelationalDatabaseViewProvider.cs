﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Query;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabaseViewProvider
    {
        public MySqlRelationalDatabaseViewProvider(IDbConnection connection, IDatabaseIdentifierDefaults identifierDefaults, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseIdentifierDefaults IdentifierDefaults { get; }

        protected IDbTypeProvider TypeProvider { get; }

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewSync(candidateViewName);
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewAsync(candidateViewName, cancellationToken);
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.views
where schema_id = schema_id(@SchemaName) and name = @ViewName";

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var resolvedViewNameOption = GetResolvedViewName(candidateViewName);
            if (resolvedViewNameOption.IsNone)
                return Option<IRelationalDatabaseView>.None;

            var resolvedViewName = resolvedViewNameOption.UnwrapSome();

            var definition = LoadDefinitionSync(resolvedViewName);
            var columns = LoadColumnsSync(resolvedViewName);
            var indexes = Array.Empty<IDatabaseIndex>();

            var view = new RelationalDatabaseView(resolvedViewName, definition, columns, indexes);
            return Option<IRelationalDatabaseView>.Some(view);
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewAsyncCore(candidateViewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var candidateViewName = QualifyViewName(viewName);
            var resolvedViewNameOption = GetResolvedViewNameAsync(candidateViewName, cancellationToken);
            var resolvedViewNameOptionIsNone = await resolvedViewNameOption.IsNone.ConfigureAwait(false);
            if (resolvedViewNameOptionIsNone)
                return Option<IRelationalDatabaseView>.None;

            var resolvedViewName = await resolvedViewNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            var columnsTask = LoadColumnsAsync(resolvedViewName, cancellationToken);
            var definitionTask = LoadDefinitionAsync(resolvedViewName, cancellationToken);
            await Task.WhenAll(columnsTask, definitionTask).ConfigureAwait(false);

            var columns = columnsTask.Result;
            var definition = definitionTask.Result;
            var indexes = Array.Empty<IDatabaseIndex>();

            var view = new RelationalDatabaseView(resolvedViewName, definition, columns, indexes);
            return Option<IRelationalDatabaseView>.Some(view);
        }

        protected virtual string LoadDefinitionSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Connection.ExecuteScalar<string>(DefinitionQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Connection.ExecuteScalarAsync<string>(DefinitionQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
        }

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select view_definition
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName";

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = new DatabaseColumn(columnName, columnType, isNullable, row.DefaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadColumnsAsyncCore(viewName, cancellationToken);
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = new DatabaseColumn(columnName, columnType, isNullable, row.DefaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
select
    column_name as ColumnName,
    data_type as DataTypeName,
    character_maximum_length as CharacterMaxLength,
    numeric_precision as `Precision`,
    numeric_scale as `Scale`,
    datetime_precision as `DateTimePrecision`,
    collation_name as Collation,
    is_nullable as IsNullable,
    column_default as DefaultValue,
    generation_expression as ComputedColumnDefinition,
    extra as ExtraInformation
from information_schema.columns
where table_schema = @SchemaName and table_name = @ViewName
order by ordinal_position";

        protected Identifier QualifyViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var schema = viewName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
        }
    }
}
