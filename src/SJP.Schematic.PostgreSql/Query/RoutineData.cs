﻿namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed class RoutineData
    {
        public string? SchemaName { get; set; }

        public string? RoutineName { get; set; }

        public string? Definition { get; set; }
    }
}
