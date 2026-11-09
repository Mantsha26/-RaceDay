using RaceDayMVC.Filters;
using RaceDayMVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options => options.Filters.Add<ApiExceptionFilter>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
    throw new InvalidOperationException("ApiSettings:BaseUrl must be an absolute API URL.");

// Local/Docker testing only: the API's ASP.NET dev certificate is not trusted inside a container.
// Enable with ApiSettings__AllowInvalidCertificate=true. Never enable this in production.
var allowInvalidCertificate = builder.Configuration.GetValue<bool>("ApiSettings:AllowInvalidCertificate");

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = apiUri;
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (allowInvalidCertificate)
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

var app = builder.Build();

// Same friendly error pages in every environment so the app behaves identically locally and in Docker.
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
if (!app.Environment.IsDevelopment()) app.UseHsts();

// The container only listens on HTTP, so the Dockerfile sets Hosting__UseHttpsRedirection=false.
if (app.Configuration.GetValue("Hosting:UseHttpsRedirection", true))
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
