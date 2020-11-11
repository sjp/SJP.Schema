﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        public class Foreign<T> : ForeignKey where T : class, new()
        {
            public Foreign(Func<T, Key> keySelector, params IModelledColumn[] columns)
                : this(keySelector, columns as IEnumerable<IModelledColumn>) { }

            public Foreign(Func<T, Key> keySelector, IEnumerable<IModelledColumn> columns)
                : base(columns)
            {
                _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            }

            public override Type TargetType { get; } = typeof(T);

            public override Func<object, Key> KeySelector
            {
                get
                {
                    if (Property == null)
                        throw new InvalidOperationException($"The { nameof(Property) } property must be set before calling { nameof(KeySelector) }.");

                    var targetProp = GetTargetProperty();
                    return obj =>
                    {
                        var result = _keySelector((T)obj);
                        result.Property = targetProp;
                        return result;
                    };
                }
            }

            // Rather ugly, but this is where the magic happens.
            // Intended to parse what the selector function really points to so that we can bind
            // a PropertyInfo object on the resulting key before we use it later via reflection.
            private PropertyInfo GetTargetProperty()
            {
                if (Property == null)
                    throw new InvalidOperationException("The property that the foreign key belongs to is not available.");

                var sourceType = Property.ReflectedType!;
                var sourceAsm = sourceType.Assembly;
                var sourceAsmName = sourceAsm.GetName();

                var sourceAsmDefinition = AssemblyCache.GetOrAdd(sourceAsmName, _ => new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(sourceAsm.Location))).Value;

                // Mono.Cecil uses '/' to declare nested type names instead of '+'
                var sourceSearchTypeName = sourceType.FullName!.Replace('+', '/');
                var sourceTypeDefinition = sourceAsmDefinition.MainModule.GetType(sourceSearchTypeName);
                var sourceProperty = sourceTypeDefinition.Properties.SingleOrDefault(p => p.Name == Property.Name && !p.HasParameters);
                if (sourceProperty == null)
                {
                    throw new ArgumentException(
                       "Could not find the source property "
                       + sourceType.FullName + "." + Property.Name
                       + ". Check that assemblies are up to date.",
                       sourceType.FullName + "." + Property.Name
                   );
                }

                var sourcePropInstructions = sourceProperty.GetMethod.Body.Instructions;
                var fnInstruction = sourcePropInstructions.FirstOrDefault(i => i.OpCode.Code == Code.Ldftn);
                if (fnInstruction == null)
                {
                    throw new ArgumentException(
                       "Could not find function pointer instruction in the get method of the source property "
                       + sourceType.FullName + "." + Property.Name
                       + ". Is the key selector method a simple lambda expression?",
                       sourceType.FullName + "." + Property.Name
                   );
                }

                if (!(fnInstruction.Operand is MethodDefinition fnOperand))
                {
                    throw new ArgumentException(
                       "Expected to find a method definition associated with a function pointer instruction but could not find one for "
                       + sourceType.FullName + "." + Property.Name + ".",
                       sourceType.FullName + "." + Property.Name
                   );
                }

                var operandInstructions = fnOperand.Body.Instructions;
                var bodyCallInstr = operandInstructions.FirstOrDefault(i => i.OpCode.Code == Code.Callvirt || i.OpCode.Code == Code.Call);
                if (bodyCallInstr == null)
                {
                    throw new ArgumentException(
                       "Could not find call or virtual call instruction in the key selector function that was provided to "
                       + sourceType.FullName + "." + Property.Name
                       + ". Is the key selector method a simple lambda expression?",
                       sourceType.FullName + "." + Property.Name
                   );
                }

                if (!(bodyCallInstr.Operand is MethodDefinition bodyMethodDef))
                    throw new ArgumentException("Expected to find a method definition associated with the call or virtual call instruction but could not find one in the key selector.");

                var targetPropertyName = bodyMethodDef.Name;
                var targetProp = TargetType.GetProperties().SingleOrDefault(p => p.GetGetMethod() != null && p.GetGetMethod()!.Name == targetPropertyName && p.GetIndexParameters().Length == 0);
                if (targetProp == null)
                {
                    throw new ArgumentException(
                       $"Expected to find a property named { targetPropertyName } in { TargetType.FullName } but could not find one.",
                       sourceType.FullName + "." + Property.Name
                   );
                }

                return targetProp;
            }

            private readonly Func<T, Key> _keySelector;
        }

        private static ConcurrentDictionary<AssemblyName, Lazy<AssemblyDefinition>> AssemblyCache { get; } = new ConcurrentDictionary<AssemblyName, Lazy<AssemblyDefinition>>();
    }
}
