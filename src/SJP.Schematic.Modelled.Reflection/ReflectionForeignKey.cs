﻿using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    // TODO: fix this so that we can point to synonyms
    public class ReflectionForeignKey : ReflectionKey
    {
        public ReflectionForeignKey(Identifier name, IDatabaseKey targetKey, IReadOnlyCollection<IDatabaseColumn> columns)
            : base(name, DatabaseKeyType.Foreign, columns)
        {
            if (targetKey == null)
                throw new ArgumentNullException(nameof(targetKey));
            if (targetKey.KeyType != DatabaseKeyType.Primary && targetKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException("The parent key given to a foreign key must be a primary or unique key. Instead given: " + targetKey.KeyType.ToString(), nameof(targetKey));
            if (columns.Count != targetKey.Columns.Count)
                throw new ArgumentException("The number of columns given to a foreign key must match the number of columns in the target key", nameof(columns));

            var columnTypes = columns.Select(static c => c.Type).ToList();
            var targetColumnTypes = targetKey.Columns.Select(static c => c.Type).ToList();

            // if we're dealing with computed columns, we can't get the types easily so avoid checking the types
            var anyComputed = columns.Any(static c => c.IsComputed) || targetKey.Columns.Any(static c => c.IsComputed);
            var columnTypesCompatible = ColumnTypesCompatible(columnTypes, targetColumnTypes);

            if (!anyComputed && !columnTypesCompatible)
                throw new ArgumentException("Incompatible column types between source and target key columns.", nameof(columns));
        }

        private static bool ColumnTypesCompatible(IEnumerable<IDbType> columnTypes, IEnumerable<IDbType> targetTypes)
        {
            return columnTypes
                .Zip(targetTypes, static (a, b) => new { Column = a, TargetColumn = b })
                .All(static cc => IsTypeEquivalent(cc.Column, cc.TargetColumn));
        }

        private static bool IsTypeEquivalent(IDbType columnType, IDbType targetType)
        {
            return columnType.ClrType == targetType.ClrType
                && (columnType.IsFixedLength == targetType.IsFixedLength || (!columnType.IsFixedLength && targetType.IsFixedLength))
                && columnType.MaxLength >= targetType.MaxLength
                && columnType.DataType == targetType.DataType;
        }
    }
}
