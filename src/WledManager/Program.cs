using System.Reflection;
using WledManager;
using WledManager.Backups;
using WledManager.Synchronization;
using WledManager.Utils.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add configuration file from container
builder.Configuration.AddJsonFile("/wled-manager/config.json", optional: true, reloadOnChange: true);

// Options
builder.Services.AddOptions<BackupOptions>().BindConfiguration("Backup");
builder.Services.AddOptions<List<PresetsSyncOptions>>().BindConfiguration("Sync");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	// Include XML comments
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
	options.IncludeXmlComments(xmlPath);
});

builder.Services.AddSingleton<TimeProvider>(svc => TimeProvider.System);
builder.Services.AddSingleton<IHealthChecksService, HealthChecksService>();
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddHostedService(svc => (BackupService)svc.GetRequiredService<IBackupService>());

builder.Services.AddSingleton<IPresetsSyncService, PresetsSyncService>();
builder.Services.AddHostedService(svc => (PresetsSyncService)svc.GetRequiredService<IPresetsSyncService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment()) ==> This is NOT designed to be exposed on Internet (like the WLED devices)
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
