using Application.Core;
using Auth.Application;
using Auth.Infrastructure;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Auth.Extensions
{
    public static class ConfigureServicesExtension
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IdService<string>, GuidGenerator>();
            services.AddScoped<ITokenService<string>, JwtService>();
            services.AddScoped<IPasswordService, PasswordGenerator>();
            services.AddScoped<ICryptoService, BcryptService>();
            services.AddScoped<IMessageBrokerService, RabbitMQService>();
            services.AddScoped<Logger, DotNetLogger>();
            services.AddScoped<IImagesCloudService<string>, CloudinaryServices>();
            services.AddSingleton<IPerformanceLogsRepository, MongoPerformanceLogsRespository>();
            services.AddSingleton<IAccountRepository, MongoAccountRepository>();
            services.AddTransient<IEmailService<EmailInfo>, SmtpService>();          
        }

        public static void ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("admin-access", p => p.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "Admin"));
                options.AddPolicy("provider-access", p => p.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "Provider"));
                options.AddPolicy("cabinOperator-access", p => p.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "CabinOperator"));
                options.AddPolicy("towdriver-access", p => p.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "TowDriver"));
                options.AddPolicy("admin-or-provider-access", policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || context.User.IsInRole("Provider"));
                });
                options.AddPolicy("admin-or-cabinOperator-or-towDriver-access", policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || context.User.IsInRole("CabinOperator") || context.User.IsInRole("TowDriver"));
                });
            });
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Audience"],
                            ClockSkew = TimeSpan.Zero,
                            RoleClaimType = ClaimTypes.Role
                        };
                    }
                );
        }

        public static void ConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();
                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URI")!), h =>
                    {
                        h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")!);
                        h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")!);
                    });

                    configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    configurator.ConfigureEndpoints(context);
                });
            });
        }

        public static void AddSwagger(this IServiceCollection services)
        { 
            services.AddSwaggerGen(c => { 
                c.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });
            });
        }

        public static void UseSwagger(this IApplicationBuilder app)
        { 
            app.UseSwagger(c => { c.SerializeAsV2 = true; });
            app.UseSwaggerUI(c => 
            { 
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth v1");
                c.RoutePrefix = string.Empty; 
            });
        }

        public static void ConfigureCors(this IServiceCollection services) 
        {
            services.AddCors(options => 
            { 
                options.AddPolicy("AllowSpecificOrigin", builder =>
                { 
                    builder.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_URL")!).AllowAnyHeader().AllowAnyMethod(); 
                }); 
            }); 
        }
    }
}