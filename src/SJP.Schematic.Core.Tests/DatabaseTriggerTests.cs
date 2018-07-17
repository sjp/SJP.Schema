﻿using System;
using NUnit.Framework;
using Moq;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseTriggerTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(table, null, definition, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, null, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            var definition = string.Empty;
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "          ";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentNullException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerQueryTiming_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = (TriggerQueryTiming)55;
            const TriggerEvent events = TriggerEvent.Update;

            Assert.Throws<ArgumentException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenInvalidTriggerEvent_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = (TriggerEvent)55;

            Assert.Throws<ArgumentException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public static void Ctor_GivenNoTriggerEvents_ThrowsArgumentException()
        {
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.None;

            Assert.Throws<ArgumentException>(() => new DatabaseTrigger(null, triggerName, definition, timing, events, true));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(table, trigger.Table);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(triggerName, trigger.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(definition, trigger.Definition);
        }

        [Test]
        public static void QueryTiming_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(timing, trigger.QueryTiming);
        }

        [Test]
        public static void TriggerEvent_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.AreEqual(events, trigger.TriggerEvent);
        }

        [Test]
        public static void IsEnabled_WhenTrueProvidedInCtor_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, true);

            Assert.IsTrue(trigger.IsEnabled);
        }

        [Test]
        public static void IsEnabled_WhenFalseProvidedInCtor_ReturnsFalse()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier triggerName = "test_trigger";
            const string definition = "create trigger test_trigger...";
            const TriggerQueryTiming timing = TriggerQueryTiming.Before;
            const TriggerEvent events = TriggerEvent.Update;

            var trigger = new DatabaseTrigger(table, triggerName, definition, timing, events, false);

            Assert.IsFalse(trigger.IsEnabled);
        }
    }
}
