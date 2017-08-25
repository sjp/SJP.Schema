﻿using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public sealed class ExcludeAttribute : ModelledSchemaAttribute
    {
        public ExcludeAttribute()
            : base(new[] { Dialect.All }) { }

        public ExcludeAttribute(params Type[] dialects)
            : base(dialects) { }
    }
}
