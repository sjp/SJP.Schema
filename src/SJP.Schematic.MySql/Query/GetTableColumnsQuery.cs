﻿using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetTableColumnsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}