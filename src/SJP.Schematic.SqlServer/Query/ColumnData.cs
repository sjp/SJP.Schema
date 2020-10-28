﻿namespace SJP.Schematic.SqlServer.Query
{
    internal sealed class ColumnData
    {
        public string ColumnName { get; set; } = default!;

        public string? ColumnTypeSchema { get; set; }

        public string ColumnTypeName { get; set; } = default!;

        public int MaxLength { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }

        public string? Collation { get; set; }

        public bool IsComputed { get; set; }

        public bool IsNullable { get; set; }

        public bool HasDefaultValue { get; set; }

        public string? DefaultValue { get; set; }

        public string? ComputedColumnDefinition { get; set; }

        public long? IdentitySeed { get; set; }

        public long? IdentityIncrement { get; set; }
    }
}
