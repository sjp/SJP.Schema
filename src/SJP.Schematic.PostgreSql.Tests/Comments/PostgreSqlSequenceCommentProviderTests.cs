﻿using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments
{
    [TestFixture]
    internal static class PostgreSqlSequenceCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlSequenceCommentProvider(null, identifierDefaults, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlSequenceCommentProvider(connection, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlSequenceCommentProvider(connection, identifierDefaults, null));
        }

        [Test]
        public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var commentProvider = new PostgreSqlSequenceCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetSequenceComments(null));
        }
    }
}