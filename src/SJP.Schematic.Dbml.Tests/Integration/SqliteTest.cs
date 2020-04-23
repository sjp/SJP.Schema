﻿using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Dbml.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new CachingConnectionFactory(new SqliteConnectionFactory(ConnectionString));

        public static ISchematicConnection Connection { get; } = new SchematicConnection(
            ConnectionFactory,
            new SqliteDialect()
        );

        private static string ConnectionString => Configuration.GetConnectionString("TestDb");

        private static IConfigurationRoot Configuration => new ConfigurationBuilder()
            .AddJsonFile("sqlite-test.config.json")
            .AddJsonFile("sqlite-test.local.config.json", optional: true)
            .Build();
    }

    [TestFixture]
    internal abstract class SqliteTest
    {
        protected ISchematicConnection Connection { get; } = Config.Connection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaultsAsync(Config.Connection).GetAwaiter().GetResult();
    }
}
