using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Services;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDataProtection();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();
// Azure when AzureStorage:ConnectionString is set; otherwise files are saved to wwwroot/uploads.
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddScoped<LocalBlobStorageService>();
builder.Services.AddScoped<IBlobStorageService>(sp =>
    string.IsNullOrWhiteSpace(sp.GetRequiredService<IConfiguration>()["AzureStorage:ConnectionString"])
        ? sp.GetRequiredService<LocalBlobStorageService>()
        : sp.GetRequiredService<AzureBlobStorageService>());
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Creates/updates RaceDayDB (all seven tables) on startup. A failure is logged
    // with the real SQL error and stops the API instead of failing later at login.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
        db.Database.Migrate();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // serves wwwroot/uploads (local storage fallback)
app.UseSession();

// The MVC application sends the protected token returned by /api/auth/login.
// It is unwrapped into the API session before SessionAuthorize filters execute.
app.Use(async (context, next) =>
{
    var authorization = context.Request.Headers.Authorization.ToString();
    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var protector = context.RequestServices
                .GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("RaceDay.ApiSession.v1");
            var values = protector.Unprotect(authorization[7..]).Split('|');
            if (values.Length == 3 &&
                int.TryParse(values[0], out var userId) &&
                long.TryParse(values[2], out var expires) &&
                expires > DateTimeOffset.UtcNow.ToUnixTimeSeconds() &&
                !string.IsNullOrWhiteSpace(values[1]))
            {
                context.Session.SetInt32("UserId", userId);
                context.Session.SetString("UserRole", values[1]);
            }
        }
        catch (CryptographicException)
        {
            // Invalid or expired tokens are treated as unauthenticated.
        }
    }

    await next();
});

app.MapControllers();
// Startup check: shows in the API console whether Azure storage is configured (never prints the value).
app.Logger.LogInformation("Environment: {Env}. Azure storage connection string configured: {Configured}",
    app.Environment.EnvironmentName,
    !string.IsNullOrWhiteSpace(app.Configuration["AzureStorage:ConnectionString"]));
if (string.IsNullOrWhiteSpace(app.Configuration["AzureStorage:ConnectionString"]))
    app.Logger.LogWarning("No Azure connection string: uploads are saved locally under wwwroot/uploads.");

app.Run();
