﻿using NUnit.Framework;
using System;
using SJP.Schematic.Core.Caching;
using System.Data;
using System.Collections.Generic;

namespace SJP.Schematic.Core.Tests.Caching
{
    [TestFixture]
    internal class CacheStoreTests
    {
        // only going to be testing basics of ctors, as this is just a wrapper for ConcurrentDictionary<TKey, TValue>
        [Test]
        public void Ctor_WhenDefaultConstructorInvoked_CreatesSuccessfully()
        {
            var store = new CacheStore<int, DataTable>();
            Assert.Pass();
        }

        [Test]
        public void Ctor_GivenNullComparer_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CacheStore<int, DataTable>((IEqualityComparer<int>)null));
        }

        [Test]
        public void Ctor_GivenNullCollection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CacheStore<int, DataTable>((IEnumerable<KeyValuePair<int, DataTable>>)null));
        }

        [Test]
        public void Ctor_GivenNullCollectionAndNullComparer_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CacheStore<int, DataTable>((IEnumerable<KeyValuePair<int, DataTable>>)null, (IEqualityComparer<int>)null));
        }
    }
}
