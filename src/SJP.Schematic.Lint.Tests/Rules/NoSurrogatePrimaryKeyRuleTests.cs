﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class NoSurrogatePrimaryKeyRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new NoSurrogatePrimaryKeyRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public static void AnalyseTables_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithSingleColumnPrimaryKey_ProducesNoMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithSingleColumnPrimaryKey_ProducesNoMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithMultiColumnPrimaryKey_ProducesMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var testColumnA = new DatabaseColumn(
                "test_column_a",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumnB = new DatabaseColumn(
                "test_column_b",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumnA, testColumnB },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithMultiColumnPrimaryKey_ProducesMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var testColumnA = new DatabaseColumn(
                "test_column_a",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumnB = new DatabaseColumn(
                "test_column_b",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumnA, testColumnB },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithMultiColumnPrimaryKeyContainingAllForeignKeyColumns_ProducesNoMessages()
        {
            var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

            var testColumnA = new DatabaseColumn(
                "test_column_a",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumnB = new DatabaseColumn(
                "test_column_b",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumnA, testColumnB },
                true
            );

            var testForeignKey1 = new DatabaseKey(
                Option<Identifier>.Some("test_fk1"),
                DatabaseKeyType.Foreign,
                new[] { testColumnA },
                true
            );
            var testForeignKey2 = new DatabaseKey(
                Option<Identifier>.Some("test_fk2"),
                DatabaseKeyType.Foreign,
                new[] { testColumnB },
                true
            );

            var relationalKey1 = new DatabaseRelationalKey(
                "test",
                testForeignKey1,
                "test",
                testPrimaryKey,
                ReferentialAction.Cascade,
                ReferentialAction.Cascade
            );
            var relationalKey2 = new DatabaseRelationalKey(
                "test",
                testForeignKey2,
                "test",
                testPrimaryKey,
                ReferentialAction.Cascade,
                ReferentialAction.Cascade
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                new[] { relationalKey1, relationalKey2 },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }
    }
}
