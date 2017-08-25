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
        public sealed class TextAttribute : DeclaredTypeAttribute
        {
            public TextAttribute()
            : base(DataType.Text, new[] { Dialect.All })
            {
            }

            public TextAttribute(params Type[] dialects)
                : base(DataType.Text, dialects)
            {
            }
        }
    }
}
