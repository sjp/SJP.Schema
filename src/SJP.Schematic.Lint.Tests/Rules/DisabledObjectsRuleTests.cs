﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class DisabledObjectsRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new DisabledObjectsRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNoDisabledObjects_ProducesNoMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithDisabledPrimaryKey_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn },
                false
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithDisabledForeignKey_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testForeignKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_foreign_key",
                DatabaseKeyType.Foreign,
                new[] { testColumn },
                false
            );
            var testPrimaryKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn },
                false
            );
            var testRelationalKey = new DatabaseRelationalKey(
                testForeignKey,
                testPrimaryKey,
                System.Data.Rule.Cascade,
                System.Data.Rule.Cascade
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                new[] { testRelationalKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithDisabledUniqueKey_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testUniqueKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_unique_key",
                DatabaseKeyType.Unique,
                new[] { testColumn },
                false
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                new[] { testUniqueKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithDisabledIndex_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testIndex = new DatabaseTableIndex(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_index",
                true,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Array.Empty<IDatabaseTableColumn>(),
                false
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { testIndex },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithDisabledCheck_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testCheck = new DatabaseCheckConstraint(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_check",
                "test_check_definition",
                false
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                new[] { testCheck },
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithDisabledTrigger_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testTrigger = new DatabaseTrigger(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_check",
                "test_check_definition",
                TriggerQueryTiming.After,
                TriggerEvent.Insert,
                false
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                new[] { testTrigger }
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
