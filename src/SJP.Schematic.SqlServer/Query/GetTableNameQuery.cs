﻿namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetTableNameQuery
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
