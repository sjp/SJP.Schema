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
                return Enumerable.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var nullableColumn in nullableColumns)
            {
                if (!IsNullDefaultValue(nullableColumn.DefaultValue))
                    continue;

                var messageText = $"The table '{ table.Name }' has a column '{ nullableColumn.Name.LocalName }' whose default value is null. Consider removing the default value on the column.";
                var ruleMessage = new RuleMessage(RuleTitle, Level, messageText);

                result.Add(ruleMessage);
            }

            return result;
        }

        protected static bool IsNullDefaultValue(string defaultValue)
        {
            return !defaultValue.IsNullOrWhiteSpace()
                && _nullValues.Contains(defaultValue);
        }

        private const string RuleTitle = "Null default values assigned to column.";
        private readonly static IEnumerable<string> _nullValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "null", "(null)" };
    }
}
