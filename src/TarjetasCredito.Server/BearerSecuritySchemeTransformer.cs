using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TarjetasCredito.Server;

/// <summary>Declara el esquema de seguridad Bearer en el documento OpenAPI para que Swagger UI muestre el botón "Authorize".</summary>
public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "opaque",
            In = ParameterLocation.Header,
            Description = "Pega aquí el accessToken devuelto por /api/auth/login (sin el prefijo 'Bearer ')."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = scheme;

        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document);
        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations!.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement { [schemeRef] = [] });
            }
        }

        return Task.CompletedTask;
    }
}
