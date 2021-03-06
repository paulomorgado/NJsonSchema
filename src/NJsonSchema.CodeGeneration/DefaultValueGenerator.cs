//-----------------------------------------------------------------------
// <copyright file="DefaultValueGenerator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Linq;

namespace NJsonSchema.CodeGeneration
{
    /// <summary>Converts the default value to a language specific identifier.</summary>
    public abstract class DefaultValueGenerator
    {
        private readonly ITypeResolver _typeResolver;

        /// <summary>Initializes a new instance of the <see cref="DefaultValueGenerator" /> class.</summary>
        /// <param name="typeResolver">The type typeResolver.</param>
        protected DefaultValueGenerator(ITypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        /// <summary>Gets the default value code.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="allowsNull">Specifies whether the default value assignment also allows null.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="typeNameHint">The type name hint to use when generating the type and the type name is missing.</param>
        /// <param name="useSchemaDefault">if set to <c>true</c> uses the default value from the schema if available.</param>
        /// <returns>The code.</returns>
        public virtual string GetDefaultValue(JsonSchema4 schema, bool allowsNull, string targetType, string typeNameHint, bool useSchemaDefault)
        {
            if (schema.Default == null || !useSchemaDefault)
                return null;

            var actualSchema = schema is JsonProperty ? ((JsonProperty)schema).ActualPropertySchema : schema.ActualSchema;
            if (actualSchema.IsEnumeration && !actualSchema.Type.HasFlag(JsonObjectType.Object) && actualSchema.Type != JsonObjectType.None)
                return GetEnumDefaultValue(schema, actualSchema, typeNameHint);

            if (schema.Type.HasFlag(JsonObjectType.String))
                return "\"" + schema.Default + "\"";
            if (schema.Type.HasFlag(JsonObjectType.Boolean))
                return schema.Default.ToString().ToLowerInvariant();
            if (schema.Type.HasFlag(JsonObjectType.Integer) ||
                    schema.Type.HasFlag(JsonObjectType.Number) ||
                    schema.Type.HasFlag(JsonObjectType.Integer))
                return schema.Default.ToString();

            return null;
        }

        /// <summary>Gets the enum default value.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="actualSchema">The actual schema.</param>
        /// <param name="typeNameHint">The type name hint.</param>
        /// <returns>The enum default value.</returns>
        protected virtual string GetEnumDefaultValue(JsonSchema4 schema, JsonSchema4 actualSchema, string typeNameHint)
        {
            var typeName = _typeResolver.Resolve(actualSchema, false, typeNameHint);

            var enumName = schema.Default is string
                ? schema.Default.ToString()
                : actualSchema.EnumerationNames[actualSchema.Enumeration.ToList().IndexOf(schema.Default)];

            return typeName + "." + ConversionUtilities.ConvertToUpperCamelCase(enumName, true);
        }
    }
}