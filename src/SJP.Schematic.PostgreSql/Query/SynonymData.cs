﻿namespace SJP.Schematic.PostgreSql.Query
{
    public class SynonymData
    {
        public string TargetServerName { get; set; }

        public string TargetDatabaseName { get; set; }

        public string TargetSchemaName { get; set; }

        public string TargetObjectName { get; set; }
    }
}