using Application.Configurations;
using Domain.Common.Constants;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Configurations;

public static class ConfigureServices
{
    public static IServiceCollection AddStartupServices(this IServiceCollection services, IConfiguration configuration)
    {
        //~ Add logging services
        services.AddLogging();


        //~ Add infrastructure services from a custom method
        services.AddInfrastructureServices(configuration);

        //~ Add controller services and configure JSON options
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            var enumConverter = new JsonStringEnumConverter();
            options.JsonSerializerOptions.Converters.Add(enumConverter);
        });

        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        //~ Add authorization configurations from a custom method
        services.AddAuthorizationConfiguration();

        //~ Add API explorer for generating API documentation
        services.AddEndpointsApiExplorer();

        //~ Add HttpContextAccessor for accessing HTTP context information
        services.AddHttpContextAccessor();

        //~ Add singleton services for IActionContextAccessor and IUrlHelperFactory
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

        //~ Add application services from a custom method
        services.AddApplicationServices(configuration);

        //~ Configure JWT settings from the application configuration
        services.Configure<Application.Common.User.Commands.Login.JWT>(configuration.GetSection("JWT"));

        //~ Add CORS policy to allow any origin, header, and method
        //services.AddCors(options =>
        //{
        //    options.AddPolicy("AllowOrigin", builder =>
        //    {
        //        builder.AllowAnyOrigin()
        //               .AllowAnyHeader()
        //               .AllowAnyMethod();
        //               //.WithExposedHeaders("Content-Disposition");
        //    });
        //});
        //services.AddCors(options =>
        //{
        //    // Define the "AllowOrigin" policy
        //    options.AddPolicy("AllowOrigin", builder =>
        //    {
        //        builder.AllowAnyOrigin()
        //               .AllowAnyHeader()
        //               .AllowAnyMethod()
        //               .WithExposedHeaders("Content-Disposition");
        //    });

        //    // Define the "AllowSpecificOrigin" policy
        //    options.AddPolicy("AllowSpecificOrigin", builder =>
        //    {
        //        builder.WithOrigins("http://localhost:4200") // Specify allowed origin
        //               .AllowAnyHeader()
        //               .AllowAnyMethod()
        //               .AllowCredentials();
        //    }); 

        //    options.AddPolicy("AllowChatOrigin", builder =>
        //    {
        //        builder.WithOrigins("http://localhost:4200", "http://localhost:51284")
        //               .AllowAnyHeader()
        //               .AllowAnyMethod()
        //               .AllowCredentials();
        //    });
        //});
        services.AddCors(options =>
        {
            options.AddPolicy("AllowServerOrigin", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
            options.AddPolicy("AllowLocalOrigin", builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });

        //~ Add Swagger/OpenAPI services for generating API documentation
        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc("Orders", new OpenApiInfo
            {
                Version = "v1",
                Title = "Orders API",
            });
            
            
            swagger.SwaggerDoc("Settings", new OpenApiInfo
            {
                Version = "v1",
                Title = "Settings API",
            });

            swagger.SwaggerDoc("Common", new OpenApiInfo
            {
                Version = "v1",
                Title = "Common API",
            });

            //~ Add custom document filters
            swagger.DocumentFilter<CustomConstantsDocumentFilter<Roles>>();
            //swagger.DocumentFilter<CustomConstantsDocumentFilter<Policies>>();

            //~ Include XML comments for Swagger
            //var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //swagger.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            //~ Add security definition for JWT Bearer authentication
            swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
            });

            //~ Add security requirement for JWT Bearer authentication
            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    //#if DEBUG || LOCAL
                    new List<string>()
                    {
                        ""
                    }
                    //#else
                    //new string[] {}
                    //#endif
                }
            });
        });

        //~ Configure IIS server options
        services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = null; //~ No limit on request body size
        });

        //~ Configure Kestrel server options
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = null; //~ No limit on request body size
            options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(90); //~ Set Keep-Alive timeout to 90 seconds
        });
       
        return services;
    }
}