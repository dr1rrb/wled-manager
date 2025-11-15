using WledManager;
using WledManager.Backups;
using WledManager.Synchronization;

var builder = WebApplication.CreateBuilder(args);

// Add configuration file from container
builder.Configuration.AddJsonFile("/wled-manager/config.json", optional: true, reloadOnChange: true);

// Options
builder.Services.AddOptions<BackupOptions>().BindConfiguration("Backup");
builder.Services.AddOptions<List<PresetsSyncOptions>>().BindConfiguration("Sync");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<TimeProvider>(svc => TimeProvider.System);
builder.Services.AddSingleton<IHealthChecksService, HealthChecksService>();
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddHostedService(svc => (BackupService)svc.GetRequiredService<IBackupService>());

builder.Services.AddSingleton<IPresetsSyncService, PresetsSyncService>();
builder.Services.AddHostedService(svc => (PresetsSyncService)svc.GetRequiredService<IPresetsSyncService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
