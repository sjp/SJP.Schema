﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseIndexColumn
    {
        IndexColumnOrder Order { get; }

        IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        string GetExpression(IDatabaseDialect dialect);
    }
}
