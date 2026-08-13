using MoMS.Server.Data;
using MoMS.Server.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Connection string comes from ConnectionStrings__MoMsConnection (or
// __DefaultConnection) machine-level env vars — never from appsettings.json.
var connectionString =
    builder.Configuration.GetConnectionString("MoMsConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No connection string configured. Set the ConnectionStrings__MoMsConnection " +
        "or ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<MoMsDbContext>(options =>
    options.UseSqlServer(connectionString));

// Domain services (Scoped — request-bound, share the scoped DbContext).
builder.Services.AddScoped<FullListService>();

// Additive schema initialization at startup.
builder.Services.AddHostedService<SchemaInitializerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Required by QuestPDF (Community licence) before any document is generated.
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Falls through to the SPA for any non-API route.
app.MapFallbackToFile("/index.html");

app.Run();
