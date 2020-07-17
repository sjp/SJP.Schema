﻿using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class LoggingTests
    {
        [Test]
        public static void IsLoggingConfigured_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.That(() => Logging.IsLoggingConfigured(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void IsLoggingConfigured_GivenConfiguredConnection_ReturnsTrue()
        {
            var dbConnection = Mock.Of<IDbConnectionFactory>();
            var connection = new SchematicConnection(dbConnection, Mock.Of<IDatabaseDialect>());

            Logging.AddLogging(connection, Mock.Of<ILogger>(), LogLevel.Information);
            Assert.That(Logging.IsLoggingConfigured(dbConnection), Is.True);
        }

        [Test]
        public static void IsLoggingConfigured_GivenNonConfiguredConnection_ReturnsFalse()
        {
            var dbConnection = Mock.Of<IDbConnectionFactory>();
            Assert.That(Logging.IsLoggingConfigured(dbConnection), Is.False);
        }

        [Test]
        public static void AddLogging_GivenNullConnection_ThrowsArgNullException()
        {
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            Assert.That(() => Logging.AddLogging(null, logger, logLevel), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenNullLogger_ThrowsArgNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            const LogLevel logLevel = LogLevel.Information;

            Assert.That(() => Logging.AddLogging(connection, null, logLevel), Throws.ArgumentNullException);
        }

        [Test]
        public static void AddLogging_GivenInvalidLogLevel_ThrowsArgException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = (LogLevel)555;

            Assert.That(() => Logging.AddLogging(connection, logger, logLevel), Throws.ArgumentException);
        }

        [Test]
        public static void RemoveLogging_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.That(() => Logging.RemoveLogging(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void RemoveLogging_WhenNoLoggingConfigured_ThrowsNothing()
        {
            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());

            Assert.That(() => Logging.RemoveLogging(connectionMock.Object), Throws.Nothing);
        }

        [Test]
        public static void RemoveLogging_WhenLoggingConfigured_ThrowsNothing()
        {
            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);
            Assert.That(() => Logging.RemoveLogging(connectionMock.Object), Throws.Nothing);
        }

        [Test]
        public static void LogCommandExecuting_WhenNoConnectionProvided_ThrowsArgumentNullException()
        {
            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);
            Assert.That(() => Logging.LogCommandExecuting(null, Guid.NewGuid(), "test_query", null), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void LogCommandExecuting_WhenNullOrWhiteSpaceSqlProvided_ThrowsArgumentNullException(string sql)
        {
            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            var logger = Mock.Of<ILogger>();
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);
            Assert.That(() => Logging.LogCommandExecuting(connectionMock.Object.DbConnection, Guid.NewGuid(), sql, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void LogCommandExecuting_WhenNoConfiguredConnectionProvided_ThrowsNothing()
        {
            var connection = Mock.Of<IDbConnectionFactory>();

            Assert.That(() => Logging.LogCommandExecuting(connection, Guid.NewGuid(), "test_query", null), Throws.Nothing);
        }

        [Test]
        public static void LogCommandExecuting_WhenValidConnectionProvided_PerformsLogging()
        {
            var connectionId = Guid.NewGuid();
            var commandId = Guid.NewGuid();
            const string sql = "test_query";
            var parameters = new { ParamA = "test_param_1", ParamB = "test_param_2" };

            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            connectionMock.Setup(c => c.ConnectionId).Returns(connectionId);
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);
            Assert.That(() => Logging.LogCommandExecuting(connectionMock.Object.DbConnection, commandId, sql, parameters), Throws.Nothing);
        }

        [Test]
        public static void LogCommandExecuting_WhenValidConnectionWithNoParamsProvided_PerformsLogging()
        {
            var connectionId = Guid.NewGuid();
            var commandId = Guid.NewGuid();
            const string sql = "test_query";

            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            connectionMock.Setup(c => c.ConnectionId).Returns(connectionId);
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);

            Assert.Multiple(() =>
            {
                Assert.That(() => Logging.LogCommandExecuting(connectionMock.Object.DbConnection, commandId, sql, null), Throws.Nothing);
                Assert.That(() => Logging.LogCommandExecuting(connectionMock.Object.DbConnection, commandId, sql, new object()), Throws.Nothing);
            });
        }

        [Test]
        public static void LogCommandExecuted_WhenValidConnectionProvided_PerformsLogging()
        {
            var connectionId = Guid.NewGuid();
            var commandId = Guid.NewGuid();
            const string sql = "test_query";
            var parameters = new { ParamA = "test_param_1", ParamB = "test_param_2" };
            var duration = TimeSpan.FromMilliseconds(1234);

            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            connectionMock.Setup(c => c.ConnectionId).Returns(connectionId);
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);
            Assert.That(() => Logging.LogCommandExecuted(connectionMock.Object.DbConnection, commandId, sql, parameters, duration), Throws.Nothing);
        }

        [Test]
        public static void LogCommandExecuted_WhenValidConnectionWithNoParamsProvided_PerformsLogging()
        {
            var connectionId = Guid.NewGuid();
            var commandId = Guid.NewGuid();
            const string sql = "test_query";
            var duration = TimeSpan.FromMilliseconds(1234);

            var connectionMock = new Mock<ISchematicConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.DbConnection).Returns(Mock.Of<IDbConnectionFactory>());
            connectionMock.Setup(c => c.ConnectionId).Returns(connectionId);
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            const LogLevel logLevel = LogLevel.Information;

            Logging.AddLogging(connectionMock.Object, logger, logLevel);

            Assert.Multiple(() =>
            {
                Assert.That(() => Logging.LogCommandExecuted(connectionMock.Object.DbConnection, commandId, sql, null, duration), Throws.Nothing);
                Assert.That(() => Logging.LogCommandExecuted(connectionMock.Object.DbConnection, commandId, sql, new object(), duration), Throws.Nothing);
            });
        }
    }
}
