﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyRelationalDatabaseTableProviderTests
    {
        [Test]
        public static void GetTable_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetTable(null));
        }

        [Test]
        public static void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetTableAsync(null));
        }

        [Test]
        public static void GetTable_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var table = provider.GetTable("table_name");

            Assert.IsTrue(table.IsNone);
        }

        [Test]
        public static async Task GetTableAsync_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var table = provider.GetTableAsync("table_name");
            var tableIsNone = await table.IsNone.ConfigureAwait(false);

            Assert.IsTrue(tableIsNone);
        }

        [Test]
        public static void Tables_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var tables = provider.Tables;

            Assert.Zero(tables.Count);
        }

        [Test]
        public static void Tables_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var tables = provider.Tables.ToList();

            Assert.Zero(tables.Count);
        }

        [Test]
        public static async Task TablesAsync_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var tables = await provider.TablesAsync().ConfigureAwait(false);

            Assert.Zero(tables.Count);
        }

        [Test]
        public static async Task TablesAsync_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var tables = await provider.TablesAsync().ConfigureAwait(false);
            var tablesList = tables.ToList();

            Assert.Zero(tablesList.Count);
        }
    }
}
