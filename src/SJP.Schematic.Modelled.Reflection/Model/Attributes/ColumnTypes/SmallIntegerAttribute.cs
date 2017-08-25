﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        public sealed class SmallIntegerAttribute : DeclaredTypeAttribute
        {
            public SmallIntegerAttribute()
            : base(DataType.SmallInteger, new[] { Dialect.All })
            {
            }

            public SmallIntegerAttribute(params Type[] dialects)
                : base(DataType.SmallInteger, dialects)
            {
            }
        }
    }
}
