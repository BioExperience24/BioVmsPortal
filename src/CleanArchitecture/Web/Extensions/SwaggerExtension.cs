using CleanArchitecture.Application.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CleanArchitecture.Web.Extensions
{
    public static class SwaggerExtension
    {
        private static readonly string[] Value = { "Bearer" };

        public static IServiceCollection AddSwaggerOpenAPI(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("OpenAPISpecification", new OpenApiInfo
                {
                    Title = appSettings.ApplicationDetail.ApplicationName,
                    Version = "v1",
                    Description = appSettings.ApplicationDetail.Description,
                    Contact = new OpenApiContact
                    {
                        Email = "",
                        Name = "",
                        Url = new Uri(appSettings.ApplicationDetail.ContactWebsite),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement { { securityScheme, Value } };
                options.AddSecurityRequirement(securityRequirement);

                // Daftarkan Filter Upload File
                options.OperationFilter<FileUploadOperationFilter>();
            });
            return services;
        }

        public class FileUploadOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // Mencari semua parameter dari metode
                var parameters = context.MethodInfo.GetParameters();
                var hasFileParameter = parameters.Any(p => p.ParameterType == typeof(IFormFile));

                if (hasFileParameter)
                {
                    // Kosongkan parameter yang ada
                    operation.Parameters.Clear();

                    // Buat request body untuk multipart/form-data
                    var requestBodySchema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    };

                    // Tambahkan parameter file dan parameter lainnya
                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType == typeof(IFormFile))
                        {
                            requestBodySchema.Properties["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            };
                        }
                        else
                        {
                            requestBodySchema.Properties[parameter.Name] = new OpenApiSchema
                            {
                                Type = GetParameterType(parameter)
                            };
                        }
                    }

                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content =
                        {
                            ["multipart/form-data"] = new OpenApiMediaType
                            {
                                Schema = requestBodySchema
                            }
                        }
                    };
                }
            }

            // Fungsi tambahan untuk menentukan tipe parameter
            private string GetParameterType(ParameterInfo parameter)
            {
                // Menentukan tipe berdasarkan jenis parameter
                return parameter.ParameterType.Name.ToLower() switch
                {
                    "string" => "string",
                    "int32" => "integer",
                    "bool" => "boolean",
                    // Tambahkan tipe lainnya jika perlu
                    _ => "string"
                };
            }
        }
    }
}
