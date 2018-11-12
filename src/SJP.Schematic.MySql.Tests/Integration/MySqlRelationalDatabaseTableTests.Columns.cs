﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal partial class MySqlRelationalDatabaseTableTests : MySqlTest
    {
        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var count = table.Columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnCollectionWithOneValue()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var count = columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Columns_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();
            var column = table.Columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();
            const string columnName = "test_column";

            Assert.AreEqual(columnName, column.Name.LocalName);
        }

        [Test]
        public void Columns_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var table = Database.GetTable("table_test_table_4").UnwrapSome();
            var columns = table.Columns;
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithMultipleColumns_ReturnsColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "middle_name", "last_name" };
            var tableOption = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var columnNames = columns.Select(c => c.Name.LocalName);

            Assert.IsTrue(expectedColumnNames.SequenceEqual(columnNames));
        }

        [Test]
        public void Columns_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNullableColumn_ColumnReturnsIsNullableTrue()
        {
            const string tableName = "table_test_table_1";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNotNullableColumn_ColumnReturnsIsNullableFalse()
        {
            const string tableName = "table_test_table_2";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public void Columns_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithColumnWithNoDefaultValue_ColumnReturnsNullDefaultValue()
        {
            const string tableName = "table_test_table_1";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.DefaultValue);
        }

        // TODO: uncomment when the default value tokens can be parsed
        //[Test]
        //public void Columns_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        //{
        //    const string tableName = "table_test_table_33";
        //    var table = Database.GetTable(tableName).UnwrapSome();
        //    var column = table.Columns.Single();
        //
        //    const string defaultValue = "1";
        //    var comparer = new SqlServerExpressionComparer();
        //    var equals = comparer.Equals(defaultValue, column.DefaultValue);
        //
        //    Assert.IsTrue(equals);
        //}
        //
        //[Test]
        //public async Task ColumnsAsync_WhenGivenTableWithColumnWithDefaultValue_ColumnReturnsCorrectDefaultValue()
        //{
        //    const string tableName = "table_test_table_33";
        //    var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
        //    var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
        //    var column = columns.Single();
        //
        //    const string defaultValue = "1";
        //    var comparer = new SqlServerExpressionComparer();
        //    var equals = comparer.Equals(defaultValue, column.DefaultValue);
        //
        //    Assert.IsTrue(equals);
        //}

        [Test]
        public void Columns_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithNonComputedColumn_ReturnsIsComputedFalse()
        {
            const string tableName = "table_test_table_1";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public void Columns_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "table_test_table_34";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithComputedColumn_ReturnsIsComputedTrue()
        {
            const string tableName = "table_test_table_34";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public void Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "table_test_table_34";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.IsNotNull(computedColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsNotNullObject()
        {
            const string tableName = "table_test_table_34";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.IsNotNull(computedColumn);
        }

        [Test]
        public void Columns_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "table_test_table_34";
            const string expectedDefinition = "(`test_column_1` + `test_column_2`)";

            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.AreEqual(expectedDefinition, computedColumn.Definition);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableWithComputedColumnCastedToInterface_ReturnsCorrectDefinition()
        {
            const string tableName = "table_test_table_34";
            const string expectedDefinition = "(`test_column_1` + `test_column_2`)";

            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            var computedColumn = column as IDatabaseComputedColumn;
            Assert.AreEqual(expectedDefinition, computedColumn.Definition);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithoutIdentity_ReturnsNullAutoincrement()
        {
            const string tableName = "table_test_table_1";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithIdentity_ReturnsNotNullAutoincrement()
        {
            const string tableName = "table_test_table_35";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.IsNotNull(column.AutoIncrement);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithIdentity_ReturnsCorrectInitialValue()
        {
            const string tableName = "table_test_table_35";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.InitialValue);
        }

        [Test]
        public void Columns_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var table = Database.GetTable(tableName).UnwrapSome();
            var column = table.Columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }

        [Test]
        public async Task ColumnsAsync_WhenGivenTableColumnWithIdentity_ReturnsCorrectIncrement()
        {
            const string tableName = "table_test_table_35";
            var tableOption = await Database.GetTableAsync(tableName).ConfigureAwait(false);
            var columns = await tableOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var column = columns.Last();

            Assert.AreEqual(1, column.AutoIncrement.Increment);
        }
    }
}