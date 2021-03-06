﻿using System;
using System.Reflection;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class ComputedColumn : IModelledComputedColumn
    {
        public ComputedColumn(string expression, object param)
        {
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            Expression = new ModelledSqlExpression(expression, param);
        }

        public virtual Type DeclaredDbType { get; } = typeof(object);

        public IModelledSqlExpression Expression { get; }

        public virtual bool IsComputed { get; } = true;

        public virtual bool IsNullable { get; } = true;

        public PropertyInfo? Property { get; set; }
    }
}
