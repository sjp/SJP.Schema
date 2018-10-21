﻿using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal sealed class PostgreSqlDatabaseSequenceTests : PostgreSqlTest
    {
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

        private IRelationalDatabase Database => new PostgreSqlRelationalDatabase(Dialect, Connection);

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseSequence(null, database, "test"));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseSequence(connection, null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseSequence(connection, database, null));
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var sequenceName = new Identifier("sequence_test_sequence_1");
            var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "sequence_test_sequence_1");

            var sequence = new PostgreSqlDatabaseSequence(Connection, database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var sequenceName = new Identifier("asd", "sequence_test_sequence_1");
            var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, "asd", "sequence_test_sequence_1");

            var sequence = new PostgreSqlDatabaseSequence(Connection, database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var sequenceName = new Identifier("qwe", "asd", "sequence_test_sequence_1");
            var expectedSequenceName = new Identifier(database.ServerName, "qwe", "asd", "sequence_test_sequence_1");

            var sequence = new PostgreSqlDatabaseSequence(Connection, database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("qwe", "asd", "zxc", "sequence_test_sequence_1");
            var expectedSequenceName = new Identifier("qwe", "asd", "zxc", "sequence_test_sequence_1");

            var sequence = new PostgreSqlDatabaseSequence(Connection, Database, sequenceName);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Start_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("db_test_sequence_1");

            Assert.AreEqual(1, sequence.Start);
        }

        [Test]
        public void Start_GivenSequenceWithCustomStart_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_2");

            Assert.AreEqual(20, sequence.Start);
        }

        [Test]
        public void Increment_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("db_test_sequence_1");

            Assert.AreEqual(1, sequence.Increment);
        }

        [Test]
        public void Increment_GivenSequenceWithCustomIncrement_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_3");

            Assert.AreEqual(100, sequence.Increment);
        }

        [Test]
        public void MinValue_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("db_test_sequence_1");

            Assert.AreEqual(1, sequence.MinValue);
        }

        [Test]
        public void MinValue_GivenSequenceWithCustomMinValue_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_4");

            Assert.AreEqual(-99, sequence.MinValue);
        }

        [Test]
        public void MinValue_GivenSequenceWithNoMinValue_ReturnsOne()
        {
            var sequence = Database.GetSequence("db_test_sequence_5");

            Assert.AreEqual(1, sequence.MinValue);
        }

        [Test]
        public void MaxValue_GivenDefaultSequence_ReturnsLongMaxValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_1");

            Assert.AreEqual(long.MaxValue, sequence.MaxValue);
        }

        [Test]
        public void MaxValue_GivenSequenceWithCustomMaxValue_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_6");

            Assert.AreEqual(333, sequence.MaxValue);
        }

        [Test]
        public void MaxValue_GivenSequenceWithNoMaxValue_ReturnsLongMaxValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_7");

            Assert.AreEqual(long.MaxValue, sequence.MaxValue);
        }

        [Test]
        public void Cycle_GivenDefaultSequence_ReturnsTrue()
        {
            var sequence = Database.GetSequence("db_test_sequence_1");

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public void Cycle_GivenSequenceWithCycle_ReturnsTrue()
        {
            var sequence = Database.GetSequence("db_test_sequence_8");

            Assert.IsTrue(sequence.Cycle);
        }

        [Test]
        public void Cycle_GivenSequenceWithNoCycle_ReturnsTrue()
        {
            var sequence = Database.GetSequence("db_test_sequence_9");

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public void Cache_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = Database.GetSequence("db_test_sequence_1");

            Assert.AreEqual(1, sequence.Cache);
        }

        [Test]
        public void Cache_GivenSequenceWithCacheSet_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_10");

            // TODO: when checks for Postgres >= 10 are available, uncomment this line
            //Assert.AreEqual(10, sequence.Cache);
            Assert.AreEqual(1, sequence.Cache);
        }

        [Test]
        public void Cache_GivenSequenceWithNoCacheSet_ReturnsCorrectValue()
        {
            var sequence = Database.GetSequence("db_test_sequence_11");

            Assert.AreEqual(1, sequence.Cache);
        }
    }
}
