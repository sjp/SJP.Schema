﻿using NUnit.Framework;
using Moq;
using System;
using System.Data;
using SJP.Schematic.Core.Caching;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal static class CachingExtensionsTests
    {
        private static Mock<IDbConnection> ConnectionMock => new Mock<IDbConnection>();

        [Test]
        public static void AsCachedConnection_GivenNullConnection_ThrowsArgNullException()
        {
            IDbConnection connection = null;
            Assert.Throws<ArgumentNullException>(() => connection.AsCachedConnection());
        }

        [Test]
        public static void AsCachedConnection_GivenValidConnection_ReturnsCachedConnectionInstance()
        {
            var connection = ConnectionMock.Object;
            var cachedConnection = connection.AsCachedConnection();

            Assert.IsInstanceOf<CachingConnection>(cachedConnection);
        }

        [Test]
        public static void AsCachedConnection_GivenNullConnectionAndValidCacheStore_ThrowsArgNullException()
        {
            IDbConnection connection = null;
            var cacheStore = new CacheStore<int, DataTable>();
            Assert.Throws<ArgumentNullException>(() => connection.AsCachedConnection(cacheStore));
        }

        [Test]
        public static void AsCachedConnection_GivenValidConnectionAndNullCacheStore_ThrowsArgNullException()
        {
            var connection = ConnectionMock.Object;
            Assert.Throws<ArgumentNullException>(() => connection.AsCachedConnection(null));
        }

        [Test]
        public static void AsCachedConnection_GivenNullConnectionAndNullCacheStore_ThrowsArgNullException()
        {
            IDbConnection connection = null;
            Assert.Throws<ArgumentNullException>(() => connection.AsCachedConnection(null));
        }

        [Test]
        public static void AsCachedConnection_GivenValidConnectionAndCacheStore_ReturnsCachedConnectionInstance()
        {
            var connection = ConnectionMock.Object;
            var cacheStore = new CacheStore<int, DataTable>();
            var cachedConnection = connection.AsCachedConnection(cacheStore);

            Assert.IsInstanceOf<CachingConnection>(cachedConnection);
        }
    }
}
