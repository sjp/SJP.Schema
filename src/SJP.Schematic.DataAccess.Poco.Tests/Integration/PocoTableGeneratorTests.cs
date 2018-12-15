﻿using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.Poco.Tests.Integration
{
    internal sealed class PocoTableGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private static IDatabaseTableGenerator TableGenerator => new PocoTableGenerator(new PascalCaseNameProvider(), TestNamespace);

        [OneTimeSetUp]
        public Task Init()
        {
            return Connection.ExecuteAsync(@"create table test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'asd'
)");
        }

        [OneTimeTearDown]
        public Task CleanUp()
        {
            return Connection.ExecuteAsync("drop table test_table_1");
        }

        [Test]
        public async Task Generate_GivenTableWithVariousColumnTypes_GeneratesExpectedOutput()
        {
            var view = await GetTable("test_table_1").ConfigureAwait(false);
            var generator = TableGenerator;

            const string expected = TestTable1Output;
            var result = generator.Generate(view);

            Assert.AreEqual(expected, result);
        }

        private const string TestNamespace = "PocoTestNamespace";

        private const string TestTable1Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_1</c> table.
    /// </summary>
    public class TestTable1
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        public long Testint { get; set; }

        /// <summary>
        /// The <c>testdecimal</c> column.
        /// </summary>
        public decimal? Testdecimal { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        public byte[] Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        public decimal? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        public string Teststring { get; set; }
    }
}";
    }
}
