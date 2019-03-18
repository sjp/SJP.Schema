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
    internal static class NoIndexesPresentOnTableRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new NoIndexesPresentOnTableRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public static void AnalyseTables_GivenTableWithoutAnyIndexesOrCandidateKeys_ProducesMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithoutAnyIndexesOrCandidateKeys_ProducesMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithOnlyPrimaryKey_ProducesNoMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                Option<IDatabaseKey>.Some(Mock.Of<IDatabaseKey>()),
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
        public static async Task AnalyseTablesAsync_GivenTableWithOnlyPrimaryKey_ProducesNoMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                Option<IDatabaseKey>.Some(Mock.Of<IDatabaseKey>()),
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
        public static void AnalyseTables_GivenTableWithOnlyUniqueKey_ProducesNoMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testUniqueKey = new DatabaseKey(
                Option<Identifier>.Some("test_unique_key"),
                DatabaseKeyType.Unique,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                Option<IDatabaseKey>.None,
                new [] { testUniqueKey },
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
        public static async Task AnalyseTablesAsync_GivenTableWithOnlyUniqueKey_ProducesNoMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testUniqueKey = new DatabaseKey(
                Option<Identifier>.Some("test_unique_key"),
                DatabaseKeyType.Unique,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                Option<IDatabaseKey>.None,
                new[] { testUniqueKey },
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
        public static void AnalyseTables_GivenTableWithOnlyIndex_ProducesNoMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                Option<IDatabaseKey>.None,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new [] { Mock.Of<IDatabaseIndex>() },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithOnlyIndex_ProducesNoMessages()
        {
            var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                Option<IDatabaseKey>.None,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { Mock.Of<IDatabaseIndex>() },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }
    }
}
