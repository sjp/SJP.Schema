﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class PluginRuleProviderTests
    {
        private static IRuleProvider RuleProvider => new PluginRuleProvider();

        [Test]
        public static void GetRules_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => RuleProvider.GetRules(null, RuleLevel.Error), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRules_GivenInvalidRuleLevel_ThrowsArgumentException()
        {
            Assert.That(() => RuleProvider.GetRules(Mock.Of<ISchematicConnection>(), (RuleLevel)555), Throws.ArgumentException);
        }

        [Test]
        public static void GetRules_WhenInvoked_RetrievesRulesFromTestRuleProvider()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = new SchematicConnection(dbConnection, dialect);

            var rules = RuleProvider.GetRules(connection, RuleLevel.Error);

            Assert.That(rules, Has.Exactly(TestRuleProvider.RuleCount).Items);
        }

        [Test]
        public static void GetRules_WhenInvokedWithMatchingDialect_RetrievesRulesFromTestRuleProviderAndTestDialectProvider()
        {
            var dbConnection = Mock.Of<IDbConnection>();
            var dialect = new Fakes.FakeDatabaseDialect();
            var connection = new SchematicConnection(dbConnection, dialect);

            var rules = RuleProvider.GetRules(connection, RuleLevel.Error);
            const int expectedCount = TestRuleProvider.RuleCount + TestDialectRuleProvider.RuleCount;

            Assert.That(rules, Has.Exactly(expectedCount).Items);
        }

        public class TestRuleProvider : IRuleProvider
        {
            public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
            {
                return new DefaultRuleProvider()
                    .GetRules(connection, level)
                    .Take(RuleCount)
                    .ToList();
            }

            public const int RuleCount = 3;
        }

        public class TestDialectRuleProvider : IDialectRuleProvider<Fakes.FakeDatabaseDialect>
        {
            public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
            {
                return new DefaultRuleProvider()
                    .GetRules(connection, level)
                    .Take(RuleCount)
                    .ToList();
            }

            public const int RuleCount = 5;
        }
    }
}