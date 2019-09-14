using System;
using System.IO;
using System.IO.Abstractions;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.OrmLite.Tests
{
    [TestFixture]
    internal static class OrmLiteTableGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrmLiteTableGenerator(null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new OrmLiteTableGenerator(nameTranslator, null));
        }

        [Test]
        public static void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new OrmLiteTableGenerator(nameTranslator, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new OrmLiteTableGenerator(nameTranslator, "   "));
        }

        [Test]
        public static void Ctor_GivenNullIndent_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new OrmLiteTableGenerator(nameTranslator, "test", null));
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(null, "test"));
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(Environment.CurrentDirectory));

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(baseDir, null));
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(Environment.CurrentDirectory));
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testTableName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(Environment.CurrentDirectory));
            const string testTableSchema = "table_schema";
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableSchema, testTableName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public static void Generate_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            var table = Mock.Of<IRelationalDatabaseTable>();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null, table, Option<IRelationalDatabaseTableComments>.None));
        }

        [Test]
        public static void Generate_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            var database = Mock.Of<IRelationalDatabase>();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(database, null, Option<IRelationalDatabaseTableComments>.None));
        }
    }
}
