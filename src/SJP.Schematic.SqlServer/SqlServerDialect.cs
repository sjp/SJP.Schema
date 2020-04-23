﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDialect : DatabaseDialect, ISqlServerDialect
    {
        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        }

        private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            return await connection.DbConnection.QuerySingleAsync<SqlIdentifierDefaults>(IdentifierDefaultsQuerySql, cancellationToken).ConfigureAwait(false);
        }

        private const string IdentifierDefaultsQuerySql = @"
select
    @@SERVERNAME as [Server],
    db_name() as [Database],
    schema_name() as [Schema]";

        public override Task<string> GetDatabaseDisplayVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.DbConnection.ExecuteScalarAsync<string>(DatabaseDisplayVersionQuerySql, cancellationToken);
        }

        private const string DatabaseDisplayVersionQuerySql = "select @@version as DatabaseVersion";

        public override Task<Version> GetDatabaseVersionAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetDatabaseVersionAsyncCore(connection, cancellationToken);
        }

        private static async Task<Version> GetDatabaseVersionAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var versionStr = await connection.DbConnection.ExecuteScalarAsync<string>(DatabaseVersionQuerySql, cancellationToken).ConfigureAwait(false);
            return Version.Parse(versionStr);
        }

        private const string DatabaseVersionQuerySql = "select SERVERPROPERTY('ProductVersion') as DatabaseVersion";

        public override Task<IRelationalDatabase> GetRelationalDatabaseAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetRelationalDatabaseAsyncCore(connection, cancellationToken);
        }

        private static async Task<IRelationalDatabase> GetRelationalDatabaseAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
            return new SqlServerRelationalDatabase(connection, identifierDefaults);
        }

        public override Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsync(ISchematicConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetRelationalDatabaseCommentProviderAsyncCore(connection, cancellationToken);
        }

        private static async Task<IRelationalDatabaseCommentProvider> GetRelationalDatabaseCommentProviderAsyncCore(ISchematicConnection connection, CancellationToken cancellationToken)
        {
            var identifierDefaults = await GetIdentifierDefaultsAsyncCore(connection, cancellationToken).ConfigureAwait(false);
            return new SqlServerDatabaseCommentProvider(connection.DbConnection, identifierDefaults);
        }

        public Task<IServerProperties2008?> GetServerProperties2008(IDbConnectionFactory connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var query = BuildServerPropertiesQuery<Query.ServerProperties2008>();
            return connection.QueryFirstOrNone<Query.ServerProperties2008>(query, cancellationToken)
                .Map<IServerProperties2008?>(row => new ServerProperties2008(row))
                .IfNoneUnsafe(() => null);
        }

        public Task<IServerProperties2012?> GetServerProperties2012(IDbConnectionFactory connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var query = BuildServerPropertiesQuery<Query.ServerProperties2012>();
            return connection.QueryFirstOrNone<Query.ServerProperties2012>(query, cancellationToken)
                .Map<IServerProperties2012?>(row => new ServerProperties2012(row))
                .IfNoneUnsafe(() => null);
        }

        public Task<IServerProperties2014?> GetServerProperties2014(IDbConnectionFactory connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var query = BuildServerPropertiesQuery<Query.ServerProperties2014>();
            return connection.QueryFirstOrNone<Query.ServerProperties2014>(query, cancellationToken)
                .Map<IServerProperties2014?>(row => new ServerProperties2014(row))
                .IfNoneUnsafe(() => null);
        }

        public Task<IServerProperties2017?> GetServerProperties2017(IDbConnectionFactory connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var query = BuildServerPropertiesQuery<Query.ServerProperties2017>();
            return connection.QueryFirstOrNone<Query.ServerProperties2017>(query, cancellationToken)
                .Map<IServerProperties2017?>(row => new ServerProperties2017(row))
                .IfNoneUnsafe(() => null);
        }

        public Task<IServerProperties2019?> GetServerProperties2019(IDbConnectionFactory connection, CancellationToken cancellationToken = default)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var query = BuildServerPropertiesQuery<Query.ServerProperties2019>();
            return connection.QueryFirstOrNone<Query.ServerProperties2019>(query, cancellationToken)
                .Map<IServerProperties2019?>(row => new ServerProperties2019(row))
                .IfNoneUnsafe(() => null);
        }

        private static string BuildServerPropertiesQuery<T>()
        {
            var propNames = typeof(T).GetProperties()
                .Select(pi => "    SERVERPROPERTY('" + pi.Name + "') AS [" + pi.Name + "]")
                .Join("," + Environment.NewLine);

            return "SELECT " + propNames;
        }

        public override IDependencyProvider GetDependencyProvider() => new SqlServerDependencyProvider();

        public override bool IsReservedKeyword(string text)
        {
            if (text.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(text));

            return Keywords.Contains(text);
        }

        // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql
        private static readonly IEnumerable<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ABSOLUTE",
            "ACTION",
            "ADA",
            "ADD",
            "ALL",
            "ALLOCATE",
            "ALTER",
            "AND",
            "ANY",
            "ARE",
            "AS",
            "ASC",
            "ASSERTION",
            "AT",
            "AUTHORIZATION",
            "AVG",
            "BACKUP",
            "BEGIN",
            "BETWEEN",
            "BIT",
            "BIT_LENGTH",
            "BOTH",
            "BREAK",
            "BROWSE",
            "BULK",
            "BY",
            "CASCADE",
            "CASCADED",
            "CASE",
            "CAST",
            "CATALOG",
            "CHAR",
            "CHAR_LENGTH",
            "CHARACTER",
            "CHARACTER_LENGTH",
            "CHECK",
            "CHECKPOINT",
            "CLOSE",
            "CLUSTERED",
            "COALESCE",
            "COLLATE",
            "COLLATION",
            "COLUMN",
            "COMMIT",
            "COMPUTE",
            "CONNECT",
            "CONNECTION",
            "CONSTRAINT",
            "CONSTRAINTS",
            "CONTAINS",
            "CONTAINSTABLE",
            "CONTINUE",
            "CONVERT",
            "CORRESPONDING",
            "COUNT",
            "CREATE",
            "CROSS",
            "CURRENT",
            "CURRENT_DATE",
            "CURRENT_TIME",
            "CURRENT_TIMESTAMP",
            "CURRENT_USER",
            "CURSOR",
            "DATABASE",
            "DATE",
            "DAY",
            "DBCC",
            "DEALLOCATE",
            "DEC",
            "DECIMAL",
            "DECLARE",
            "DEFAULT",
            "DEFERRABLE",
            "DEFERRED",
            "DELETE",
            "DENY",
            "DESC",
            "DESCRIBE",
            "DESCRIPTOR",
            "DIAGNOSTICS",
            "DISCONNECT",
            "DISK",
            "DISTINCT",
            "DISTRIBUTED",
            "DOMAIN",
            "DOUBLE",
            "DROP",
            "DUMP",
            "ELSE",
            "END",
            "END-EXEC",
            "ERRLVL",
            "ESCAPE",
            "EXCEPT",
            "EXCEPTION",
            "EXEC",
            "EXECUTE",
            "EXISTS",
            "EXIT",
            "EXTERNAL",
            "EXTRACT",
            "FALSE",
            "FETCH",
            "FILE",
            "FILLFACTOR",
            "FIRST",
            "FLOAT",
            "FOR",
            "FOREIGN",
            "FORTRAN",
            "FOUND",
            "FREETEXT",
            "FREETEXTTABLE",
            "FROM",
            "FULL",
            "FUNCTION",
            "GET",
            "GLOBAL",
            "GO",
            "GOTO",
            "GRANT",
            "GROUP",
            "HAVING",
            "HOLDLOCK",
            "HOUR",
            "IDENTITY",
            "IDENTITY_INSERT",
            "IDENTITYCOL",
            "IF",
            "IMMEDIATE",
            "IN",
            "INCLUDE",
            "INDEX",
            "INDICATOR",
            "INITIALLY",
            "INNER",
            "INPUT",
            "INSENSITIVE",
            "INSERT",
            "INT",
            "INTEGER",
            "INTERSECT",
            "INTERVAL",
            "INTO",
            "IS",
            "ISOLATION",
            "JOIN",
            "KEY",
            "KILL",
            "LANGUAGE",
            "LAST",
            "LEADING",
            "LEFT",
            "LEVEL",
            "LIKE",
            "LINENO",
            "LOAD",
            "LOCAL",
            "LOWER",
            "MATCH",
            "MAX",
            "MERGE",
            "MIN",
            "MINUTE",
            "MODULE",
            "MONTH",
            "NAMES",
            "NATIONAL",
            "NATURAL",
            "NCHAR",
            "NEXT",
            "NO",
            "NOCHECK",
            "NONCLUSTERED",
            "NONE",
            "NOT",
            "NULL",
            "NULLIF",
            "NUMERIC",
            "OCTET_LENGTH",
            "OF",
            "OFF",
            "OFFSETS",
            "ON",
            "ONLY",
            "OPEN",
            "OPENDATASOURCE",
            "OPENQUERY",
            "OPENROWSET",
            "OPENXML",
            "OPTION",
            "OR",
            "ORDER",
            "OUTER",
            "OUTPUT",
            "OVER",
            "OVERLAPS",
            "PAD",
            "PARTIAL",
            "PASCAL",
            "PERCENT",
            "PIVOT",
            "PLAN",
            "POSITION",
            "PRECISION",
            "PREPARE",
            "PRESERVE",
            "PRIMARY",
            "PRINT",
            "PRIOR",
            "PRIVILEGES",
            "PROC",
            "PROCEDURE",
            "PUBLIC",
            "RAISERROR",
            "READ",
            "READTEXT",
            "REAL",
            "RECONFIGURE",
            "REFERENCES",
            "RELATIVE",
            "REPLICATION",
            "RESTORE",
            "RESTRICT",
            "RETURN",
            "REVERT",
            "REVOKE",
            "RIGHT",
            "ROLLBACK",
            "ROWCOUNT",
            "ROWGUIDCOL",
            "ROWS",
            "RULE",
            "SAVE",
            "SCHEMA",
            "SCROLL",
            "SECOND",
            "SECTION",
            "SECURITYAUDIT",
            "SELECT",
            "SEMANTICKEYPHRASETABLE",
            "SEMANTICSIMILARITYDETAILSTABLE",
            "SEMANTICSIMILARITYTABLE",
            "SESSION",
            "SESSION_USER",
            "SET",
            "SETUSER",
            "SHUTDOWN",
            "SIZE",
            "SMALLINT",
            "SOME",
            "SPACE",
            "SQL",
            "SQLCA",
            "SQLCODE",
            "SQLERROR",
            "SQLSTATE",
            "SQLWARNING",
            "STATISTICS",
            "SUBSTRING",
            "SUM",
            "SYSTEM_USER",
            "TABLE",
            "TABLESAMPLE",
            "TEMPORARY",
            "TEXTSIZE",
            "THEN",
            "TIME",
            "TIMESTAMP",
            "TIMEZONE_HOUR",
            "TIMEZONE_MINUTE",
            "TO",
            "TOP",
            "TRAILING",
            "TRAN",
            "TRANSACTION",
            "TRANSLATE",
            "TRANSLATION",
            "TRIGGER",
            "TRIM",
            "TRUE",
            "TRUNCATE",
            "TRY_CONVERT",
            "TSEQUAL",
            "UNION",
            "UNIQUE",
            "UNKNOWN",
            "UNPIVOT",
            "UPDATE",
            "UPDATETEXT",
            "UPPER",
            "USAGE",
            "USE",
            "USER",
            "USING",
            "VALUE",
            "VALUES",
            "VARCHAR",
            "VARYING",
            "VIEW",
            "WAITFOR",
            "WHEN",
            "WHENEVER",
            "WHERE",
            "WHILE",
            "WITH",
            "WITHIN GROUP",
            "WORK",
            "WRITE",
            "WRITETEXT",
            "YEAR",
            "ZONE"
        };

        public override string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"[{ identifier.Replace("]", "]]") }]";
        }

        public override string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Server != null)
                pieces.Add(QuoteIdentifier(name.Server));
            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }

        public override IDbTypeProvider TypeProvider => InnerTypeProvider;

        private static readonly IDbTypeProvider InnerTypeProvider = new SqlServerDbTypeProvider();
    }
}
