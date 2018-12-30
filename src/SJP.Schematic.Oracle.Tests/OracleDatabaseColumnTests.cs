﻿using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseColumnTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseColumn(null, columnType, true, null));
        }

        [Test]
        public static void Ctor_GivenNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseColumn("test_column", null, true, null));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();

            var column = new OracleDatabaseColumn(columnName, columnType, true, null);

            Assert.AreEqual(columnName, column.Name);
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();

            var column = new OracleDatabaseColumn(columnName, columnType, true, null);

            Assert.AreEqual(columnType, column.Type);
        }

        [Test]
        public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseColumn(columnName, columnType, false, null);

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseColumn(columnName, columnType, true, null);

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public static void DefaultValue_PropertyGet_ReturnsCtorArg()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string defaultValue = "1";
            var column = new OracleDatabaseColumn(columnName, columnType, true, defaultValue);

            Assert.AreEqual(defaultValue, column.DefaultValue.UnwrapSome());
        }

        [Test]
        public static void IsComputed_PropertyGet_ReturnsFalse()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseColumn(columnName, columnType, true, null);

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public static void AutoIncrement_PropertyGet_ReturnsNone()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseColumn(columnName, columnType, true, null);

            Assert.IsTrue(column.AutoIncrement.IsNone);
        }
    }
}
