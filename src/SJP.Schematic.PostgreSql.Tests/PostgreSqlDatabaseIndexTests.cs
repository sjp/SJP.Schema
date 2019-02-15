﻿using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseIndexTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndex(null, isUnique, columns));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndex(indexName, isUnique, null));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns));
        }

        [Test]
        public static void Ctor_GivenNullIncludedColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns, null));
        }

        [Test]
        public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumns = new IDatabaseColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns, includedColumns));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.AreEqual(indexName, index.Name);
        }

        [Test]
        public static void IsUnique_WithTrueCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public static void IsUnique_WithFalseCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.AreEqual(columns, index.Columns);
        }

        [Test]
        public static void IncludedColumns_PropertyGetWhenNotProvidedInCtor_IsEmpty()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.Zero(index.IncludedColumns.Count);
        }

        [Test]
        public static void IncludedColumns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.AreEqual(includedColumns, index.IncludedColumns);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.IsTrue(index.IsEnabled);
        }
    }
}
