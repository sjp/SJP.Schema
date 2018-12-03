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
    internal static class NoNonNullableColumnsPresentRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new NoNonNullableColumnsPresentRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new NoNonNullableColumnsPresentRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNotNullableColumns_ProducesNoMessages()
        {
            var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var testColumn3 = new DatabaseColumn(
                "test_column_3",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2, testColumn3 },
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNoNullableColumns_ProducesMessages()
        {
            var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }
    }
}
