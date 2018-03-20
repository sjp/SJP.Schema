﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteViewGenerator : DatabaseViewGenerator
    {
        public OrmLiteViewGenerator(INameProvider nameProvider, string baseNamespace)
            : base(nameProvider)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameProvider.SchemaToNamespace(view.Name);
            var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = view.Columns
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != viewNamespace)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            namespaces.Add("ServiceStack.DataAnnotations");

            var builder = new StringBuilder();
            foreach (var ns in namespaces)
            {
                builder.Append("using ")
                    .Append(ns)
                    .AppendLine(";");
            }

            if (namespaces.Count > 0)
                builder.AppendLine();

            builder.Append("namespace ")
                .AppendLine(viewNamespace)
                .AppendLine("{");

            // todo configure for tabs?
            const string viewIndent = IndentLevel;

            var viewComment = GenerateViewComment(view.Name.LocalName);
            builder.AppendComment(viewIndent, viewComment);

            var schemaName = view.Name.Schema;
            if (!schemaName.IsNullOrWhiteSpace())
            {
                var schemaNameLiteral = schemaName.ToStringLiteral();
                builder.Append(viewIndent)
                    .Append("[Schema(")
                    .Append(schemaNameLiteral)
                    .AppendLine(")]");
            }

            var className = NameProvider.ViewToClassName(view.Name);
            if (className != view.Name.LocalName)
            {
                var aliasName = view.Name.LocalName.ToStringLiteral();
                builder.Append(viewIndent)
                    .Append("[Alias(")
                    .Append(aliasName)
                    .AppendLine(")]");
            }

            builder.Append(viewIndent)
                .Append("public class ")
                .AppendLine(className)
                .Append(viewIndent)
                .AppendLine("{");

            const string columnIndent = viewIndent + IndentLevel;
            var hasFirstLine = false;
            foreach (var column in view.Columns)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                AppendColumn(builder, columnIndent, className, column);
                hasFirstLine = true;
            }

            builder.Append(viewIndent)
                .AppendLine("}")
                .Append("}");

            return builder.ToString();
        }

        protected virtual string GenerateViewComment(string viewName)
        {
            if (viewName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            var escapedViewName = SecurityElement.Escape(viewName);
            return "A mapping class to query the <c>" + escapedViewName + "</c> view.";
        }

        protected virtual string GenerateColumnComment(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var escapedColumnName = SecurityElement.Escape(columnName);
            return "The <c>" + escapedColumnName + "</c> column.";
        }

        private void AppendColumn(StringBuilder builder, string columnIndent, string className, IDatabaseViewColumn column)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (columnIndent == null)
                throw new ArgumentNullException(nameof(columnIndent));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var clrType = column.Type.ClrType;
            var nullableSuffix = clrType.IsValueType && column.IsNullable ? "?" : string.Empty;

            var typeName = clrType.Name;
            if (clrType.Namespace == "System" && _typeNameMap.ContainsKey(typeName))
                typeName = _typeNameMap[typeName];

            var columnComment = GenerateColumnComment(column.Name.LocalName);
            builder.AppendComment(columnIndent, columnComment);

            var propertyName = NameProvider.ColumnToPropertyName(className, column.Name.LocalName);
            if (propertyName != column.Name.LocalName)
            {
                var aliasName = column.Name.LocalName.ToStringLiteral();
                builder.Append(columnIndent)
                    .Append("[Alias(")
                    .Append(aliasName)
                    .AppendLine(")]");
            }

            builder.Append(columnIndent)
                .Append("public ")
                .Append(typeName)
                .Append(nullableSuffix)
                .Append(" ")
                .Append(propertyName)
                .AppendLine(" { get; set; }");
        }

        private const string IndentLevel = "    ";

        private readonly static IReadOnlyDictionary<string, string> _typeNameMap = new Dictionary<string, string>
        {
            ["Boolean"] = "bool",
            ["Byte"] = "byte",
            ["Byte[]"] = "byte[]",
            ["SByte"] = "sbyte",
            ["Char"] = "char",
            ["Decimal"] = "decimal",
            ["Double"] = "double",
            ["Single"] = "float",
            ["Int32"] = "int",
            ["UInt32"] = "uint",
            ["Int64"] = "long",
            ["UInt64"] = "ulong",
            ["Object"] = "object",
            ["Int16"] = "short",
            ["UInt16"] = "ushort",
            ["String"] = "string"
        };
    }
}
