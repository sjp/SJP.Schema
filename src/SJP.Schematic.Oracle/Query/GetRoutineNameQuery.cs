﻿namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetRoutineNameQuery
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }
}
