﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal sealed class PostgreSqlRelationalDatabaseViewProviderTests : PostgreSqlTest
    {
        private IRelationalDatabaseViewProvider ViewProvider => new PostgreSqlRelationalDatabaseViewProvider(Connection, IdentifierDefaults, IdentifierResolver, Dialect.TypeProvider);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_2 as select table_id as test from view_test_table_1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view db_test_view_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        private Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            lock (_lock)
            {
                if (!_viewsCache.TryGetValue(viewName, out var lazyView))
                {
                    lazyView = new AsyncLazy<IRelationalDatabaseView>(() => ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync());
                    _viewsCache[viewName] = lazyView;
                }

                return lazyView.Task;
            }
        }

        private readonly static object _lock = new object();
        private readonly static ConcurrentDictionary<Identifier, AsyncLazy<IRelationalDatabaseView>> _viewsCache = new ConcurrentDictionary<Identifier, AsyncLazy<IRelationalDatabaseView>>();

        [Test]
        public async Task GetViewAsync_WhenViewPresent_ReturnsView()
        {
            var viewIsSome = await ViewProvider.GetViewAsync("db_test_view_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresent_ReturnsViewWithCorrectName()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewMissing_ReturnsNone()
        {
            var viewIsNone = await ViewProvider.GetViewAsync("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public async Task ViewsAsync_WhenEnumerated_ContainsViews()
        {
            var views = await ViewProvider.ViewsAsync().ConfigureAwait(false);

            Assert.NotZero(views.Count);
        }

        [Test]
        public async Task ViewsAsync_WhenEnumerated_ContainsTestView()
        {
            const string viewName = "db_test_view_1";
            var views = await ViewProvider.ViewsAsync().ConfigureAwait(false);
            var containsTestView = views.Any(v => v.Name.LocalName == viewName);

            Assert.True(containsTestView);
        }

        [Test]
        public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);

            var definition = view.Definition;
            const string expected = " SELECT 1 AS test;";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public async Task IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var view = await GetViewAsync("view_test_view_1").ConfigureAwait(false);

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public async Task Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = await GetViewAsync("view_test_view_1").ConfigureAwait(false);
            var indexCount = view.Indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var containsColumn = view.Columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }
    }
}
