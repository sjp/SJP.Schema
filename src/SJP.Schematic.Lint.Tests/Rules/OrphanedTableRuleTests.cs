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
    internal static class OrphanedTableRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new OrphanedTableRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public static void AnalyseTables_GivenTableWithParentKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var parentKey = new DatabaseRelationalKey(
                "child_table",
                new DatabaseKey(
                    Option<Identifier>.Some("child_key"),
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                "parent_table",
                new DatabaseKey(
                    Option<Identifier>.Some("parent_key"),
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            );
            var childTable = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                new[] { parentKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { childTable };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithParentKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var parentKey = new DatabaseRelationalKey(
                "child_table",
                new DatabaseKey(
                    Option<Identifier>.Some("child_key"),
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                "parent_table",
                new DatabaseKey(
                    Option<Identifier>.Some("parent_key"),
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            );
            var childTable = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                new[] { parentKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { childTable };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithChildKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var childKey = new DatabaseRelationalKey(
                "child_table",
                new DatabaseKey(
                    Option<Identifier>.Some("child_key"),
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                "parent_table",
                new DatabaseKey(
                    Option<Identifier>.Some("parent_key"),
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            );
            var parentTable = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { childKey },
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { parentTable };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithChildKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var childKey = new DatabaseRelationalKey(
                "child_table",
                new DatabaseKey(
                    Option<Identifier>.Some("child_key"),
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                "parent_table",
                new DatabaseKey(
                    Option<Identifier>.Some("parent_key"),
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            );
            var parentTable = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { childKey },
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { parentTable };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithNoRelations_ProducesMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

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
        public static async Task AnalyseTablesAsync_GivenTableWithNoRelations_ProducesMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

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
    }
}
