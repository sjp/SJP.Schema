﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.SchemaSpy.Html.Lint.Rules
{
    internal class NoSurrogatePrimaryKeyRule : Schematic.Lint.Rules.NoSurrogatePrimaryKeyRule
    {
        public NoSurrogatePrimaryKeyRule(RuleLevel level)
            : base(level)
        {
        }

        protected override IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableLink = $"<a href=\"tables/{ tableName.ToSafeKey() }.html\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";
            var messageText = $"The table { tableLink } has a multi-column primary key. Consider introducing a surrogate primary key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }
    }
}
