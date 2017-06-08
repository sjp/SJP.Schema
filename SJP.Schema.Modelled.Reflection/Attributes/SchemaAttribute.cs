﻿using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public sealed class SchemaAttribute : AutoSchemaAttribute
    {
        public SchemaAttribute(string schema)
            : base(new[] { Dialect.All })
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            Schema = schema;
        }

        public SchemaAttribute(string schema, params Type[] dialects)
            : base(dialects)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            Schema = schema;
        }

        public string Schema { get; }
    }
}
