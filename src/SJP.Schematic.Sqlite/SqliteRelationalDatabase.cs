﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Query;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, string defaultSchema = "main")
            : base(dialect, connection)
        {
            if (defaultSchema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(defaultSchema));

            Metadata = new DatabaseMetadata { DatabaseName = connection.Database, DefaultSchema = defaultSchema };
        }

        public string ServerName => null; // never not-null

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata { get; }

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            if (tableName.Schema != null)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(tableName.Schema) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
                return Connection.ExecuteScalar<int>(
                    sql,
                    new { TableName = tableName.LocalName }
                ) > 0;
            }

            var dbNames = DatabaseNames.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
                var tableCount = Connection.ExecuteScalar<int>(sql, new { TableName = tableName.LocalName });

                if (tableCount > 0)
                    return true;
            }

            return false;
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return false;

            if (tableName.Schema != null)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(tableName.Schema) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
                return await Connection.ExecuteScalarAsync<int>(
                    sql,
                    new { TableName = tableName.LocalName }
                ).ConfigureAwait(false) > 0;
            }

            var dbNamesResult = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'table' and lower(name) = lower(@TableName)";
                var tableCount = await Connection.ExecuteScalarAsync<int>(sql, new { TableName = tableName.LocalName }).ConfigureAwait(false);

                if (tableCount > 0)
                    return true;
            }

            return false;
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return null;

            if (tableName.Schema != null)
                return LoadTableSync(tableName);

            var dbNames = DatabaseNames.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = new Identifier(dbName, tableName.LocalName);
                var table = LoadTableSync(qualifiedTableName);

                if (table != null)
                    return table;
            }

            return null;
        }

        public async Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (BuiltInTables.Contains(tableName.LocalName))
                return null;

            if (tableName.Schema != null)
                return await LoadTableAsync(tableName).ConfigureAwait(false);

            var dbNamesResult = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = new Identifier(dbName, tableName.LocalName);
                var table = await LoadTableAsync(qualifiedTableName).ConfigureAwait(false);

                if (table != null)
                    return table;
            }

            return null;
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                var dbNames = DatabaseNames
                    .Select(l => l.name)
                    .OrderBy(n => n)
                    .ToList();

                foreach (var dbName in dbNames)
                {
                    var sql = $"select name from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'table' order by name";
                    var tableNames = Connection.Query<string>(sql)
                        .Where(name => !BuiltInTables.Contains(name))
                        .Select(name => new Identifier(dbName, name));

                    foreach (var tableName in tableNames)
                    {
                        var table = LoadTableSync(tableName);
                        if (table != null)
                            yield return table;
                    }
                }
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync()
        {
            var dbNamesQuery = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesQuery
                .Select(l => l.name)
                .OrderBy(n => n)
                .ToList();

            var result = Enumerable.Empty<IRelationalDatabaseTable>().ToAsyncEnumerable();

            foreach (var dbName in dbNames)
            {
                var sql = $"select name from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'table' order by name";
                var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
                var tableNames = queryResult
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new Identifier(dbName, name));

                foreach (var tableName in tableNames)
                {
                    var table = LoadTableSync(tableName);
                    if (table != null)
                        result = result.Append(table);
                }
            }

            return result;
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Schema != null)
            {
                return TableExists(tableName)
                    ? new SqliteRelationalDatabaseTable(Connection, this, tableName)
                    : null;
            }

            var dbNames = DatabaseNames.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = new Identifier(dbName, tableName.LocalName);
                var table = TableExists(qualifiedTableName)
                    ? new SqliteRelationalDatabaseTable(Connection, this, qualifiedTableName)
                    : null;

                if (table != null)
                    return table;
            }

            return null;
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (tableName.Schema != null)
            {
                var exists = await TableExistsAsync(tableName).ConfigureAwait(false);
                return exists
                    ? new SqliteRelationalDatabaseTable(Connection, this, tableName)
                    : null;
            }

            var dbNamesResult = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedTableName = new Identifier(dbName, tableName.LocalName);
                var table = TableExists(qualifiedTableName)
                    ? new SqliteRelationalDatabaseTable(Connection, this, qualifiedTableName)
                    : null;

                if (table != null)
                    return table;
            }

            return null;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(viewName.Schema) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
                return Connection.ExecuteScalar<int>(
                    sql,
                    new { ViewName = viewName.LocalName }
                ) > 0;
            }

            var dbNames = DatabaseNames.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
                var viewCount = Connection.ExecuteScalar<int>(sql, new { ViewName = viewName.LocalName });

                if (viewCount > 0)
                    return true;
            }

            return false;
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(viewName.Schema) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
                return await Connection.ExecuteScalarAsync<int>(
                    sql,
                    new { ViewName = viewName.LocalName }
                ).ConfigureAwait(false) > 0;
            }

            var dbNamesResult = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var sql = $"select count(*) from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'view' and lower(name) = lower(@ViewName)";
                var viewCount = await Connection.ExecuteScalarAsync<int>(sql, new { ViewName = viewName.LocalName }).ConfigureAwait(false);

                if (viewCount > 0)
                    return true;
            }

            return false;
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
                return LoadViewSync(viewName);

            var dbNames = DatabaseNames.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = new Identifier(dbName, viewName.LocalName);
                var view = LoadViewSync(qualifiedViewName);

                if (view != null)
                    return view;
            }

            return null;
        }

        public async Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
                return await LoadViewAsync(viewName).ConfigureAwait(false);

            var dbNamesResult = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = new Identifier(dbName, viewName.LocalName);
                var view = await LoadViewAsync(qualifiedViewName).ConfigureAwait(false);

                if (view != null)
                    return view;
            }

            return null;
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                var dbNames = DatabaseNames
                    .Select(l => l.name)
                    .OrderBy(n => n)
                    .ToList();

                foreach (var dbName in dbNames)
                {
                    var sql = $"select name from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'view' order by name";
                    var viewNames = Connection.Query<string>(sql)
                        .Where(name => !BuiltInTables.Contains(name))
                        .Select(name => new Identifier(dbName, name));

                    foreach (var viewName in viewNames)
                    {
                        var view = LoadViewSync(viewName);
                        if (view != null)
                            yield return view;
                    }
                }
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync()
        {
            var dbNamesQuery = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesQuery
                .Select(l => l.name)
                .OrderBy(n => n)
                .ToList();

            var result = Enumerable.Empty<IRelationalDatabaseView>().ToAsyncEnumerable();

            foreach (var dbName in dbNames)
            {
                var sql = $"select name from { Dialect.QuoteIdentifier(dbName) }.sqlite_master where type = 'view' order by name";
                var queryResult = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
                var viewNames = queryResult
                    .Where(name => !BuiltInTables.Contains(name))
                    .Select(name => new Identifier(dbName, name));

                foreach (var viewName in viewNames)
                {
                    var view = await LoadViewAsync(viewName).ConfigureAwait(false);
                    if (view != null)
                        result = result.Append(view);
                }
            }

            return result;
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var exists = ViewExists(viewName);
                return exists
                    ? new SqliteRelationalDatabaseView(Connection, this, viewName)
                    : null;
            }

            var dbNames = DatabaseNames.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = new Identifier(dbName, viewName.LocalName);
                var view = ViewExists(qualifiedViewName)
                    ? new SqliteRelationalDatabaseView(Connection, this, qualifiedViewName)
                    : null;

                if (view != null)
                    return view;
            }

            return null;
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            if (viewName.Schema != null)
            {
                var exists = await ViewExistsAsync(viewName).ConfigureAwait(false);
                return exists
                    ? new SqliteRelationalDatabaseView(Connection, this, viewName)
                    : null;
            }

            var dbNamesResult = await DatabaseNamesAsync().ConfigureAwait(false);
            var dbNames = dbNamesResult.OrderBy(l => l.seq).Select(l => l.name).ToList();
            foreach (var dbName in dbNames)
            {
                var qualifiedViewName = new Identifier(dbName, viewName.LocalName);
                var exists = await ViewExistsAsync(qualifiedViewName).ConfigureAwait(false);
                var view = exists
                    ? new SqliteRelationalDatabaseView(Connection, this, qualifiedViewName)
                    : null;

                if (view != null)
                    return view;
            }

            return null;
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return false;
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return null;
        }

        public IEnumerable<IDatabaseSequence> Sequences => Enumerable.Empty<IDatabaseSequence>();

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Task.FromResult(false);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Task.FromResult<IDatabaseSequence>(null);
        }

        public Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync() => Task.FromResult(Enumerable.Empty<IDatabaseSequence>().ToAsyncEnumerable());

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return false;
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return null;
        }

        public IEnumerable<IDatabaseSynonym> Synonyms => Enumerable.Empty<IDatabaseSynonym>();

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Task.FromResult(false);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Task.FromResult<IDatabaseSynonym>(null);
        }

        public Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync() => Task.FromResult(Enumerable.Empty<IDatabaseSynonym>().ToAsyncEnumerable());

        #endregion Synonyms

        protected IEnumerable<DatabaseList> DatabaseNames => Connection.Query<DatabaseList>("PRAGMA database_list");

        protected Task<IEnumerable<DatabaseList>> DatabaseNamesAsync() => Connection.QueryAsync<DatabaseList>("PRAGMA database_list");

        protected static IEnumerable<string> BuiltInTables { get; } = new HashSet<string>(new[] { "sqlite_master", "sqlite_temp_master", "sqlite_sequence" }, StringComparer.OrdinalIgnoreCase);
    }
}
