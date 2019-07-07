﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyIndexRule : Rule, ITableRule
    {
        public ForeignKeyIndexRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var indexes = table.Indexes;
            var foreignKeys = table.ParentKeys.Select(fk => fk.ChildKey).ToList();

            foreach (var foreignKey in foreignKeys)
            {
                var columns = foreignKey.Columns;

                var isIndexedKey = indexes.Select(i => i.Columns).Any(ic => ColumnsHaveIndex(columns, ic));
                if (!isIndexedKey)
                {
                    var columnNames = columns.Select(c => c.Name.LocalName).ToList();
                    var message = BuildMessage(foreignKey.Name, table.Name, columnNames);
                    result.Add(message);
                }
            }

            return result;
        }

        protected static bool ColumnsHaveIndex(IEnumerable<IDatabaseColumn> columns, IEnumerable<IDatabaseIndexColumn> indexColumns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (indexColumns == null)
                throw new ArgumentNullException(nameof(indexColumns));

            var columnList = columns.ToList();
            var dependentColumns = indexColumns.SelectMany(ic => ic.DependentColumns).ToList();

            // can only check for regular indexes, not functional ones (functions may be composed of multiple columns)
            if (columnList.Count != dependentColumns.Count)
                return false;

            var columnNames = columnList.Select(c => c.Name).ToList();
            var indexColumnNames = dependentColumns.Select(ic => ic.Name).ToList();

            return IsPrefixOf(columnNames, indexColumnNames);
        }

        protected static bool IsPrefixOf<T>(IEnumerable<T> prefixSet, IEnumerable<T> otherSet)
        {
            if (prefixSet == null)
                throw new ArgumentNullException(nameof(prefixSet));
            if (otherSet == null)
                throw new ArgumentNullException(nameof(otherSet));

            var prefixSetList = prefixSet.ToList();
            if (prefixSetList.Empty())
                throw new ArgumentException("The given prefix set contained no values.", nameof(prefixSet));

            var superSetList = otherSet.ToList();
            if (superSetList.Empty())
                throw new ArgumentException("The given super set contained no values.", nameof(otherSet));

            if (prefixSetList.Count > superSetList.Count)
                return false;

            if (superSetList.Count > prefixSetList.Count)
                superSetList = superSetList.Take(prefixSetList.Count).ToList();

            return prefixSetList.SequenceEqual(superSetList);
        }

        protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier tableName, IEnumerable<string> columnNames)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnNames == null || columnNames.Empty())
                throw new ArgumentNullException(nameof(columnNames));

            var builder = StringBuilderCache.Acquire();
            builder.Append("The table ")
                .Append(tableName)
                .Append(" has a foreign key ");

            foreignKeyName.IfSome(name =>
            {
                builder.Append("'")
                    .Append(name.LocalName)
                    .Append("' ");
            });

            builder.Append("which is missing an index on the column");

            // plural check
            if (columnNames.Skip(1).Any())
                builder.Append("s");

            var joinedColumnNames = columnNames.Join(", ");
            builder.Append(" ")
                .Append(joinedColumnNames);

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Indexes missing on foreign key.";
    }
}
