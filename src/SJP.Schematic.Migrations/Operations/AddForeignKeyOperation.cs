﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AddForeignKeyOperation : MigrationOperation
    {
        public AddForeignKeyOperation(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            ForeignKey = foreignKey ?? throw new ArgumentNullException(nameof(foreignKey));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseRelationalKey ForeignKey { get; }
    }
}
