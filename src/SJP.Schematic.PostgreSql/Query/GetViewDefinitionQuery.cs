﻿namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record GetViewDefinitionQuery
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
