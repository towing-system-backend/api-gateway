using Application.Core;
using Auth.Extensions;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.ConfigureServices();
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureMassTransit(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddSwagger();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy =>
    {
        policy.RequireRole("Admin");
    });
});
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseCors("AllowAll");
app.UseAuthorization();
app.UseSwagger();

app.MapGet("api/auth/health", () => Results.Ok("ok"));

app.MapReverseProxy().RequireAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
