﻿using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class DatabaseComputedColumn : DatabaseColumn, IDatabaseComputedColumn
    {
        public DatabaseComputedColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, string definition)
            : base(columnName, type, isNullable, defaultValue, null)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
        }

        public string Definition { get; }

        public override bool IsComputed { get; } = true;
    }
}
