﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseKey
    {
        IRelationalDatabaseTable Table { get; }

        Identifier Name { get; }

        IEnumerable<IDatabaseColumn> Columns { get; }

        DatabaseKeyType KeyType { get; }
    }
}
