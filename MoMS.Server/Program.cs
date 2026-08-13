using MoMS.Server.Data;
using MoMS.Server.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MoMsConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
var connectionConfigured = !string.IsNullOrWhiteSpace(connectionString);

if (!connectionConfigured)
    connectionString = "Server=(unconfigured);Database=(unconfigured);Trusted_Connection=True;TrustServerCertificate=True";

builder.Services.AddDbContext<MoMsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<FullListService>();
builder.Services.AddScoped<FullListRepeatService>();
builder.Services.AddScoped<ListHistoryService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<TimelineService>();
builder.Services.AddScoped<PreparationService>();
builder.Services.AddScoped<ImageStorageService>();
builder.Services.AddScoped<DocketService>();
builder.Services.AddScoped<ListOptionService>();

builder.Services.AddHostedService<SchemaInitializerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

var staticRoot = Path.Combine(app.Environment.WebRootPath, "static");
Directory.CreateDirectory(Path.Combine(staticRoot, "toolset_img"));
Directory.CreateDirectory(Path.Combine(staticRoot, "docket_pdf"));

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();