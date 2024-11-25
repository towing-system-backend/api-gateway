using Application.Core;
using Auth.Application;
using Auth.Infrastructure;
using DotNetEnv;
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
builder.Services.AddSingleton<IAccountRepository, MongoAccountRepository>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin-access", p => p.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "admin"));
    
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
