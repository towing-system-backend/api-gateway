using Application.Core;
using Auth.Application;
using Auth.Infrastructure;
using DotNetEnv;
using MassTransit;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddScoped<IdService<string>, GuidGenerator>();
builder.Services.AddScoped<ITokenService<string>, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordGenerator>();
builder.Services.AddScoped<ICryptoService, BcryptService>();
builder.Services.AddTransient<IEmailService<EmailInfo>, SmtpService>();
builder.Services.AddScoped<IMessageBrokerService, RabbitMQService>();
builder.Services.AddSingleton<IAccountRepository, MongoAccountRepository>();
builder.Services.AddScoped<Logger, DotNetLogger>();
builder.Services.AddScoped<IPerformanceLogsRepository, MongoPerformanceLogsRespository>();
builder.Services.AddControllers();

var certSection = builder.Configuration.GetSection("Kestrel:Endpoints:Https:Certificate");
certSection["Path"] = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel_CertificatesDefault_Path")!;
certSection["Password"] = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel_CertificatesDefault_Password")!;

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin-access", p => p.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "Admin"));
    
});
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddMassTransit(busConfigurator =>
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();


app.UseSwagger(c =>
{
    c.SerializeAsV2 = true;
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth v1");
    c.RoutePrefix = string.Empty;
});

app.MapGet("api/auth/health", () => Results.Ok("ok"));

app.MapReverseProxy().RequireAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
