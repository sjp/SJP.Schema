﻿using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.That(() => new MySqlCheckConstraint(null, "test", true), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
        {
            Identifier checkName = "test_check";

            Assert.That(() => new MySqlCheckConstraint(checkName, definition, true), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new MySqlCheckConstraint(checkName, "test_definition", true);

            Assert.That(check.Name.UnwrapSome(), Is.EqualTo(checkName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            const string definition = "test_definition";
            var check = new MySqlCheckConstraint(checkName, definition, true);

            Assert.That(check.Definition, Is.EqualTo(definition));
        }

        [Test]
        public static void IsEnabled_PropertyGetGivenTrueCtorArg_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            const string definition = "test_definition";
            var check = new MySqlCheckConstraint(checkName, definition, true);

            Assert.That(check.IsEnabled, Is.True);
        }

        [Test]
        public static void IsEnabled_PropertyGetGivenFalseCtorArg_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            const string definition = "test_definition";
            var check = new MySqlCheckConstraint(checkName, definition, false);

            Assert.That(check.IsEnabled, Is.False);
        }
    }
}
