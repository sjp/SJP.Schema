﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDialect : DatabaseDialect<SqlServerDialect>
    {
        public override IDbConnection CreateConnection(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            var builder = new SqlConnectionStringBuilder(connectionString) { MultipleActiveResultSets = true };
            var connWithMars = builder.ConnectionString;

            var connection = new SqlConnection(connWithMars);
            connection.Open();
            return connection;
        }

        public override Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            var builder = new SqlConnectionStringBuilder(connectionString) { MultipleActiveResultSets = true };
            var connWithMars = builder.ConnectionString;

            return CreateConnectionAsyncCore(connWithMars, cancellationToken);
        }

        private static async Task<IDbConnection> CreateConnectionAsyncCore(string connectionString, CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public override IIdentifierDefaults GetIdentifierDefaults(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.QuerySingle<IdentifierDefaults>(IdentifierDefaultsQuerySql);
        }

        public override Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return GetIdentifierDefaultsAsyncCore(connection, cancellationToken);
        }

        private static async Task<IIdentifierDefaults> GetIdentifierDefaultsAsyncCore(IDbConnection connection, CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<IdentifierDefaults>(IdentifierDefaultsQuerySql).ConfigureAwait(false);
        }

        private const string IdentifierDefaultsQuerySql = @"
select
    @@SERVERNAME as [Server],
    db_name() as [Database],
    schema_name() as [Schema]";

        public override string GetDatabaseVersion(IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.ExecuteScalar<string>(DatabaseVersionQuerySql);
        }

        public override Task<string> GetDatabaseVersionAsync(IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.ExecuteScalarAsync<string>(DatabaseVersionQuerySql);
        }

        private const string DatabaseVersionQuerySql = "select @@version as DatabaseVersion";

        public override bool IsReservedKeyword(string text)
        {
            if (text.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(text));

            return _keywords.Contains(text);
        }

        // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql
        private readonly static IEnumerable<string> _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        public override IDbTypeProvider TypeProvider => _typeProvider;

        private readonly static IDbTypeProvider _typeProvider = new SqlServerDbTypeProvider();
    }
}
