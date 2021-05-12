﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetSequenceDefinitionQueryResult
    {
        public int CacheSize { get; init; }

        public string? Cycle { get; init; }

        public decimal Increment { get; init; }

        public decimal MinValue { get; init; }

        public decimal MaxValue { get; init; }
    }
}