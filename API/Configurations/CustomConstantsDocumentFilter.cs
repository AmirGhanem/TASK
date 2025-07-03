using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Domain.Common.Constants;
using System.Reflection;

namespace Configurations
{
    public class CustomConstantsDocumentFilter<TClass> : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var constClassType = typeof(TClass);
            var constValues = new Dictionary<string, OpenApiSchema>();

            foreach (FieldInfo fieldInfo in constClassType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    var fieldName = fieldInfo.Name;
                    var fieldValue = fieldInfo.GetValue(null)?.ToString();

                    if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldValue))
                    {
                        constValues[fieldName] = new OpenApiSchema
                        {
                            Type = "string",
                            Description = $"{fieldValue}", // Add a description if needed
                            // Enum = new List<IOpenApiAny> { new OpenApiString(fieldValue) }
                        };
                    }
                }
            }

            swaggerDoc.Components.Schemas.Add(typeof(TClass).Name, new OpenApiSchema
            {
                Type = "object",
                Properties = constValues
            });
        }
    }
}