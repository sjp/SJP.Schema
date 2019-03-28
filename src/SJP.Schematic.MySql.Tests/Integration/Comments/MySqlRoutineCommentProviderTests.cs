﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Comments;

namespace SJP.Schematic.MySql.Tests.Integration.Comments
{
    internal sealed class MySqlRoutineCommentProviderTests : MySqlTest
    {
        private IDatabaseRoutineCommentProvider RoutineCommentProvider => new MySqlRoutineCommentProvider(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
CREATE FUNCTION comment_test_routine_1()
  RETURNS TEXT
  LANGUAGE SQL
BEGIN
  RETURN 'test';
END").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
CREATE PROCEDURE comment_test_routine_2()
BEGIN
   COMMIT;
END").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
CREATE FUNCTION comment_test_routine_3()
  RETURNS TEXT
  LANGUAGE SQL
  COMMENT 'This is a test function comment.'
BEGIN
  RETURN 'test';
END
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
CREATE PROCEDURE comment_test_routine_4()
  COMMENT 'This is a test stored procedure comment.'
BEGIN
   COMMIT;
END
").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop function comment_test_routine_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop function comment_test_routine_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop procedure comment_test_routine_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop procedure comment_test_routine_4").ConfigureAwait(false);
        }

        private Task<IDatabaseRoutineComments> GetRoutineCommentsAsync(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            lock (_lock)
            {
                if (!_commentsCache.TryGetValue(routineName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseRoutineComments>(() => RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync());
                    _commentsCache[routineName] = lazyComment;
                }

                return lazyComment.Task;
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>>();

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineComment()
        {
            var routineIsSome = await RoutineCommentProvider.GetRoutineComments("comment_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(routineIsSome);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "comment_test_routine_1";
            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(routineName, routineComments.RoutineName.LocalName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(routineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutineMissing_ReturnsNone()
        {
            var routineIsNone = await RoutineCommentProvider.GetRoutineComments("routine_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(routineIsNone);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenEnumerated_ContainsRoutineComments()
        {
            var routineComments = await RoutineCommentProvider.GetAllRoutineComments().ConfigureAwait(false);

            Assert.NotZero(routineComments.Count);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenEnumerated_ContainsTestRoutineComment()
        {
            var routineComments = await RoutineCommentProvider.GetAllRoutineComments().ConfigureAwait(false);
            var containsTestRoutine = routineComments.Any(t => t.RoutineName.LocalName == "comment_test_routine_1");

            Assert.IsTrue(containsTestRoutine);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("comment_test_routine_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test function comment.";
            var comments = await GetRoutineCommentsAsync("comment_test_routine_3").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, routineComment);
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("comment_test_routine_2").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test stored procedure comment.";
            var comments = await GetRoutineCommentsAsync("comment_test_routine_4").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, routineComment);
        }
    }
}