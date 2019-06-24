﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AddUniqueKeyOperation : MigrationOperation
    {
        public AddUniqueKeyOperation(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            UniqueKey = uniqueKey ?? throw new ArgumentNullException(nameof(uniqueKey));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseKey UniqueKey { get; }
    }
}
