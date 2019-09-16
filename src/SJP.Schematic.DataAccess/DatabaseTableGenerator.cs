﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess
{
    public abstract class DatabaseTableGenerator : IDatabaseTableGenerator
    {
        protected DatabaseTableGenerator(INameTranslator nameTranslator, string indent = "    ")
        {
            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Indent = indent ?? throw new ArgumentNullException(nameof(indent));
        }

        protected INameTranslator NameTranslator { get; }

        protected string Indent { get; }

        public abstract string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment);

        public virtual FileInfoBase GetFilePath(IDirectoryInfo baseDirectory, Identifier objectName)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));

            var paths = new List<string> { baseDirectory.FullName, "Tables" };
            if (objectName.Schema != null)
            {
                var schemaName = NameTranslator.SchemaToNamespace(objectName);
                paths.Add(schemaName);
            }

            var tableName = NameTranslator.TableToClassName(objectName);
            paths.Add(tableName + ".cs");

            var tablePath = Path.Combine(paths.ToArray());
            return new FileInfo(tablePath);
        }
    }
}
