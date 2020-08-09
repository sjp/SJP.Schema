﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Versions.V12
{
    /// <summary>
    /// A database table provider for PostgreSQL v12 and higher.
    /// </summary>
    /// <seealso cref="V11.PostgreSqlRelationalDatabaseTableProvider" />
    public class PostgreSqlRelationalDatabaseTableProvider : V11.PostgreSqlRelationalDatabaseTableProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">A database identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
        public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
            : base(connection, identifierDefaults, identifierResolver)
        {
        }

        /// <summary>
        /// Retrieves the columns for a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An ordered collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected override Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var query = await DbConnection.QueryAsync<ColumnDataV12>(
                ColumnsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(Constants.PgCatalog, row.data_type),
                    Collation = !row.collation_name.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.collation_catalog, row.collation_schema, row.collation_name))
                        : Option<Identifier>.None,
                    MaxLength = row.character_maximum_length > 0
                        ? row.character_maximum_length
                        : CreatePrecisionFromBase(row.numeric_precision, row.numeric_precision_radix),
                    NumericPrecision = row.numeric_precision_radix > 0
                        ? Option<INumericPrecision>.Some(CreatePrecisionWithScaleFromBase(row.numeric_precision, row.numeric_scale, row.numeric_precision_radix))
                        : Option<INumericPrecision>.None
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.column_name);

                var isAutoIncrement = row.is_identity == Constants.Yes;
                var autoIncrement = isAutoIncrement
                    && decimal.TryParse(row.identity_start, out var seqStart)
                    && decimal.TryParse(row.identity_increment, out var seqIncr)
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(seqStart, seqIncr))
                    : Option<IAutoIncrement>.None;

                var isSerialAutoIncrement = !isAutoIncrement && !row.serial_sequence_schema_name.IsNullOrWhiteSpace() && !row.serial_sequence_local_name.IsNullOrWhiteSpace();
                if (isSerialAutoIncrement)
                    autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(1, 1));

                var defaultValue = !row.column_default.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.column_default)
                    : Option<string>.None;
                var isNullable = row.is_nullable == Constants.Yes;

                var isComputed = row.is_generated == Constants.Always;
                var computedDefinition = isComputed
                    ? Option<string>.Some(row.generation_expression ?? string.Empty)
                    : Option<string>.None;

                var column = isComputed
                    ? new DatabaseComputedColumn(columnName, columnType, isNullable, defaultValue, computedDefinition)
                    : new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves column definitions.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string ColumnsQuery => ColumnsQuerySql;

        // a little bit convoluted due to the quote_ident() being required.
        // when missing, case folding will occur (we should have guaranteed that this is already done)
        // additionally the default behaviour misses the schema which may be necessary
        private static readonly string ColumnsQuerySql = @$"
select
    column_name as ""{ nameof(ColumnDataV12.column_name) }"",
    ordinal_position as ""{ nameof(ColumnDataV12.ordinal_position) }"",
    column_default as ""{ nameof(ColumnDataV12.column_default) }"",
    is_nullable as ""{ nameof(ColumnDataV12.is_nullable) }"",
    data_type as ""{ nameof(ColumnDataV12.data_type) }"",
    character_maximum_length as ""{ nameof(ColumnDataV12.character_maximum_length) }"",
    character_octet_length as ""{ nameof(ColumnDataV12.character_octet_length) }"",
    numeric_precision as ""{ nameof(ColumnDataV12.numeric_precision) }"",
    numeric_precision_radix as ""{ nameof(ColumnDataV12.numeric_precision_radix) }"",
    numeric_scale as ""{ nameof(ColumnDataV12.numeric_scale) }"",
    datetime_precision as ""{ nameof(ColumnDataV12.datetime_precision) }"",
    interval_type as ""{ nameof(ColumnDataV12.interval_type) }"",
    collation_catalog as ""{ nameof(ColumnDataV12.collation_catalog) }"",
    collation_schema as ""{ nameof(ColumnDataV12.collation_schema) }"",
    collation_name as ""{ nameof(ColumnDataV12.collation_name) }"",
    domain_catalog as ""{ nameof(ColumnDataV12.domain_catalog) }"",
    domain_schema as ""{ nameof(ColumnDataV12.domain_schema) }"",
    domain_name as ""{ nameof(ColumnDataV12.domain_name) }"",
    udt_catalog as ""{ nameof(ColumnDataV12.udt_catalog) }"",
    udt_schema as ""{ nameof(ColumnDataV12.udt_schema) }"",
    udt_name as ""{ nameof(ColumnDataV12.udt_name) }"",
    dtd_identifier as ""{ nameof(ColumnDataV12.dtd_identifier) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[1] as ""{ nameof(ColumnDataV12.serial_sequence_schema_name) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[2] as ""{ nameof(ColumnDataV12.serial_sequence_local_name) }"",
    is_identity as ""{ nameof(ColumnDataV12.is_identity) }"",
    identity_generation as ""{ nameof(ColumnDataV12.identity_generation) }"",
    identity_start as ""{ nameof(ColumnDataV12.identity_start) }"",
    identity_increment as ""{ nameof(ColumnDataV12.identity_increment) }"",
    identity_maximum as ""{ nameof(ColumnDataV12.identity_maximum) }"",
    identity_minimum as ""{ nameof(ColumnDataV12.identity_minimum) }"",
    identity_cycle as ""{ nameof(ColumnDataV12.identity_cycle) }"",
    is_generated as ""{ nameof(ColumnDataV12.is_generated) }"",
    generation_expression as ""{ nameof(ColumnDataV12.generation_expression) }""
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

        /// <summary>
        /// Retrieves check constraints defined on a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of check constraints.</returns>
        protected override async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            var checks = await DbConnection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            const string checkPrefix = "CHECK (";
            const string checkSuffix = ")";
            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var definition = checkRow.Definition;
                if (definition.IsNullOrWhiteSpace())
                    continue;

                if (definition.StartsWith(checkPrefix, StringComparison.OrdinalIgnoreCase))
                    definition = definition.Substring(checkPrefix.Length);
                if (definition.EndsWith(checkSuffix, StringComparison.Ordinal) && definition.Length > 0)
                    definition = definition.Substring(0, definition.Length - checkSuffix.Length);

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);

                var check = new PostgreSqlCheckConstraint(constraintName, definition);
                result.Add(check);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves check constraint information for a table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string ChecksQuery => ChecksQuerySql;

        private static readonly string ChecksQuerySql = @$"
select
    c.conname as ""{ nameof(CheckConstraintData.ConstraintName) }"",
    pg_catalog.pg_get_constraintdef(c.oid) as ""{ nameof(CheckConstraintData.Definition) }""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";
    }
}
