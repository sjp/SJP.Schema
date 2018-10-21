﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseSynonym : IDatabaseSynonym
    {
        public OracleDatabaseSynonym(IRelationalDatabase database, Identifier synonymName, Identifier targetName)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var serverName = synonymName.Server ?? database.ServerName;
            var databaseName = synonymName.Database ?? database.DatabaseName;
            var schemaName = synonymName.Schema ?? database.DefaultSchema;

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, synonymName.LocalName);

            var targetServerName = targetName.Server ?? database.ServerName;
            var targetDatabaseName = targetName.Database ?? database.DatabaseName;
            var targetSchemaName = targetName.Schema ?? database.DefaultSchema;

            Target = Identifier.CreateQualifiedIdentifier(targetServerName, targetDatabaseName, targetSchemaName, targetName.LocalName); // don't check for validity of target, could be a broken synonym
        }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
