﻿using System;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDatabaseTrigger : IDatabaseTrigger
    {
        public SqliteDatabaseTrigger(Identifier name, string definition, TriggerQueryTiming queryTiming, TriggerEvent events)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (!queryTiming.IsValid())
                throw new ArgumentException($"The { nameof(TriggerQueryTiming) } provided must be a valid enum.", nameof(queryTiming));
            if (!events.IsValid())
                throw new ArgumentException($"The { nameof(TriggerEvent) } provided must be a valid enum.", nameof(events));
            if (events == TriggerEvent.None)
                throw new ArgumentException("Invalid trigger event flags given. Must include at least one event, e.g. INSERT, DELETE, UPDATE.", nameof(events));

            Name = name.LocalName;
            Definition = definition;
            QueryTiming = queryTiming;
            TriggerEvent = events;
        }

        public string Definition { get; }

        public Identifier Name { get; }

        public TriggerQueryTiming QueryTiming { get; }

        public TriggerEvent TriggerEvent { get; }

        public bool IsEnabled { get; } = true; // always enabled for sqlite
    }
}
