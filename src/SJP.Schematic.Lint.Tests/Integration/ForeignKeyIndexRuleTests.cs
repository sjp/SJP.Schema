﻿using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class ForeignKeyIndexRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table no_index_parent_table_1 ( column_1 integer not null primary key autoincrement )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table indexed_child_table_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_indexed_child_table_1 on indexed_child_table_1 (column_2)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table indexed_child_table_2 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_indexed_child_table_2 on indexed_child_table_2 (column_2, column_1)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table not_indexed_child_table_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table no_index_parent_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table indexed_child_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table indexed_child_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table not_indexed_child_table_1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyIndexRule(level));
        }

        [Test]
        public static void AnalyseDatabaseAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabaseAsync(null));
        }

        [Test]
        public async Task AnalyseDatabaseAsync_GivenDatabaseWithTableWithIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = GetSqliteDatabase();

            fakeDatabase.Tables = new[]
            {
                await database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("indexed_child_table_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseDatabaseAsync(fakeDatabase).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseDatabaseAsync_GivenDatabaseWithTableWithoutIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = GetSqliteDatabase();

            fakeDatabase.Tables = new[]
            {
                await database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("not_indexed_child_table_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseDatabaseAsync(fakeDatabase).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }
    }
}
