﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using PgSequenceProvider = SJP.Schematic.PostgreSql.Versions.V10.PostgreSqlDatabaseSequenceProvider;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V10
{
    internal sealed class PostgreSqlDatabaseSequenceProviderTests : PostgreSql10Test
    {
        private IDatabaseSequenceProvider SequenceProvider => new PgSequenceProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create sequence db_test_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_2 start with 20").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_3 start with 100 increment by 100").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_4 start with 1000 minvalue -99").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_5 start with 1000 no minvalue").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_6 start with 1 maxvalue 333").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_7 start with 1 no maxvalue").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_8 cycle").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_9 no cycle").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_10 cache 10").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_11 cache 1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop sequence db_test_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_9").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_11").ConfigureAwait(false);
        }

        private Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            lock (_lock)
            {
                if (!_sequencesCache.TryGetValue(sequenceName, out var lazySequence))
                {
                    lazySequence = new AsyncLazy<IDatabaseSequence>(() => SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync());
                    _sequencesCache[sequenceName] = lazySequence;
                }

                return lazySequence.Task;
            }
        }

        private readonly static object _lock = new object();
        private readonly static ConcurrentDictionary<Identifier, AsyncLazy<IDatabaseSequence>> _sequencesCache = new ConcurrentDictionary<Identifier, AsyncLazy<IDatabaseSequence>>();

        [Test]
        public async Task GetSequence_WhenSequencePresent_ReturnsSequence()
        {
            var sequenceIsSome = await SequenceProvider.GetSequence("db_test_sequence_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(sequenceIsSome);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresent_ReturnsSequenceWithCorrectName()
        {
            const string sequenceName = "db_test_sequence_1";
            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(sequenceName, sequence.Name.LocalName);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(sequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

            var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequence_WhenSequenceMissing_ReturnsNone()
        {
            var sequenceIsNone = await SequenceProvider.GetSequence("sequence_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(sequenceIsNone);
        }

        [Test]
        public async Task GetAllSequences_WhenEnumerated_ContainsSequences()
        {
            var sequences = await SequenceProvider.GetAllSequences().ConfigureAwait(false);

            Assert.NotZero(sequences.Count);
        }

        [Test]
        public async Task GetAllSequences_WhenEnumerated_ContainsTestSequence()
        {
            var sequences = await SequenceProvider.GetAllSequences().ConfigureAwait(false);
            var containsTestSequence = sequences.Any(s => s.Name.LocalName == "db_test_sequence_1");

            Assert.IsTrue(containsTestSequence);
        }

        [Test]
        public async Task Start_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

            Assert.AreEqual(1, sequence.Start);
        }

        [Test]
        public async Task Start_GivenSequenceWithCustomStart_ReturnsCorrectValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_2").ConfigureAwait(false);

            Assert.AreEqual(20, sequence.Start);
        }

        [Test]
        public async Task Increment_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

            Assert.AreEqual(1, sequence.Increment);
        }

        [Test]
        public async Task Increment_GivenSequenceWithCustomIncrement_ReturnsCorrectValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_3").ConfigureAwait(false);

            Assert.AreEqual(100, sequence.Increment);
        }

        [Test]
        public async Task MinValue_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

            Assert.AreEqual(1, sequence.MinValue.UnwrapSome());
        }

        [Test]
        public async Task MinValue_GivenSequenceWithCustomMinValue_ReturnsCorrectValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_4").ConfigureAwait(false);

            Assert.AreEqual(-99, sequence.MinValue.UnwrapSome());
        }

        [Test]
        public async Task MinValue_GivenSequenceWithNoMinValue_ReturnsOne()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_5").ConfigureAwait(false);

            Assert.AreEqual(1, sequence.MinValue.UnwrapSome());
        }

        [Test]
        public async Task MaxValue_GivenDefaultSequence_ReturnsLongMaxValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

            Assert.AreEqual(long.MaxValue, sequence.MaxValue.UnwrapSome());
        }

        [Test]
        public async Task MaxValue_GivenSequenceWithCustomMaxValue_ReturnsCorrectValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_6").ConfigureAwait(false);

            Assert.AreEqual(333, sequence.MaxValue.UnwrapSome());
        }

        [Test]
        public async Task MaxValue_GivenSequenceWithNoMaxValue_ReturnsLongMaxValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_7").ConfigureAwait(false);

            Assert.AreEqual(long.MaxValue, sequence.MaxValue.UnwrapSome());
        }

        [Test]
        public async Task Cycle_GivenDefaultSequence_ReturnsTrue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public async Task Cycle_GivenSequenceWithCycle_ReturnsTrue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_8").ConfigureAwait(false);

            Assert.IsTrue(sequence.Cycle);
        }

        [Test]
        public async Task Cycle_GivenSequenceWithNoCycle_ReturnsTrue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_9").ConfigureAwait(false);

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public async Task Cache_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

            Assert.AreEqual(1, sequence.Cache);
        }

        [Test]
        public async Task Cache_GivenSequenceWithCacheSet_ReturnsCorrectValue()
        {
            const int expectedCache = 10;
            var sequence = await GetSequenceAsync("db_test_sequence_10").ConfigureAwait(false);

            Assert.AreEqual(expectedCache, sequence.Cache);
        }

        [Test]
        public async Task Cache_GivenSequenceWithNoCacheSet_ReturnsCorrectValue()
        {
            var sequence = await GetSequenceAsync("db_test_sequence_11").ConfigureAwait(false);

            Assert.AreEqual(1, sequence.Cache);
        }
    }
}