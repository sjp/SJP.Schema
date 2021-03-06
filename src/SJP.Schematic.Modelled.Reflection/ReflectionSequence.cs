﻿using System;
using System.Linq;
using System.Reflection;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSequence : IDatabaseSequence
    {
        public ReflectionSequence(IRelationalDatabase database, IDatabaseDialect dialect, Type sequenceType)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            var typeInfo = sequenceType.GetTypeInfo();
            if (!typeInfo.ImplementedInterfaces.Contains(ISequenceType))
                throw new ArgumentException($"The sequence type { typeInfo.FullName } must implement the { ISequenceType.FullName } interface.", nameof(sequenceType));
            var ctor = sequenceType.GetDefaultConstructor();
            if (ctor == null)
                throw new ArgumentException($"The sequence type { typeInfo.FullName } does not contain a default constructor.", nameof(sequenceType));

            var instance = ctor.Invoke(Array.Empty<object>()) as ISequence;
            var sequenceName = dialect.GetQualifiedNameOrDefault(database, sequenceType);

            var minValue = instance!.MinValue.ToOption();
            var maxValue = instance!.MaxValue.ToOption();

            // create an inner sequence, which will perform validation
            var sequence = new DatabaseSequence(
                sequenceName,
                instance!.Start,
                instance!.Increment,
                minValue,
                maxValue,
                instance!.Cycle,
                instance!.Cache
            );

            Cache = sequence.Cache;
            Cycle = sequence.Cycle;
            Increment = sequence.Increment;
            MaxValue = maxValue;
            MinValue = minValue;
            Name = sequenceName;
            Start = sequence.Start;
        }

        public int Cache { get; }

        public bool Cycle { get; }

        public decimal Increment { get; }

        public Option<decimal> MaxValue { get; }

        public Option<decimal> MinValue { get; }

        public Identifier Name { get; }

        public decimal Start { get; }

        private static readonly TypeInfo ISequenceType = typeof(ISequence).GetTypeInfo();
    }
}
