﻿using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules
{
    public class ColumnWithNullDefaultValueRule : Rule
    {
        public ColumnWithNullDefaultValueRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
            if (nullableColumns.Count == 0)
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                if (!IsNullDefaultValue(nullableColumn.DefaultValue))
                    continue;

                var ruleMessage = BuildMessage(table.Name, nullableColumn.Name.LocalName);
                result.Add(ruleMessage);
            }

            return result;
        }

        protected static bool IsNullDefaultValue(string defaultValue)
        {
            return !defaultValue.IsNullOrWhiteSpace()
                && _nullValues.Contains(defaultValue);
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var messageText = $"The table '{ tableName }' has a column '{ columnName }' whose default value is null. Consider removing the default value on the column.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Null default values assigned to column.";
        private readonly static IEnumerable<string> _nullValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "null", "(null)" };
    }
}
