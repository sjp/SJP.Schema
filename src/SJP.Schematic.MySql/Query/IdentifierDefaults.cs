﻿using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    public class IdentifierDefaults : IDatabaseIdentifierDefaults
    {
        public string Server { get; set; }

        public string Database { get; set; }

        public string Schema { get; set; }
    }
}
