using Microsoft.Extensions.Options;
using System.Text.Json;
using WledManager.DTO;
using WledManager.Utils;
using WledManager.Utils.HealthChecks;

namespace WledManager.Backups;

public sealed class BackupService(IOptionsMonitor<BackupOptions> options, TimeProvider time, IHealthChecksService health, ILogger<BackupService> log) : BackgroundService, IBackupService
{
	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (options.CurrentValue.StartDelay is { } delay)
		{
			if (delay > TimeSpan.Zero)
			{
				await Task.Delay(delay, stoppingToken);
			}

			await BackupAsync(stoppingToken);
		}

		var timer = SetupTimer(options.CurrentValue, stoppingToken);
		options
			.OnChange(opts => Interlocked.Exchange(ref timer, SetupTimer(opts, stoppingToken)).Dispose())
			?.DisposeWith(stoppingToken);

		stoppingToken.Register(() => timer.Dispose()); // No method group to make sure to dispose the latest timer!
	}

	private IDisposable SetupTimer(BackupOptions opts, CancellationToken ct)
	{
		var timeOfDay = time.GetLocalNow().TimeOfDay;
		var due = opts.TimeOfDay - timeOfDay;
		while (due < TimeSpan.Zero)
		{
			due += TimeSpan.FromDays(1);
		}

		return time.CreateTimer(_ => _ = BackupAsync(ct), null, due, TimeSpan.FromDays(1));
	}

	public async Task BackupAsync(CancellationToken ct)
	{
		var opts = options.CurrentValue;
		var path = opts.Path;

		log.LogTrace("Starting backup of WLED configs and presets.");

		await using var hc = health.StartNew(opts.Health);

		foreach (var dev in opts.Devices)
		{
			log.LogTrace("Starting backup for device '{dev}'.", dev);

			using var device = new WledClient(dev);

			try
			{
				var name = dev;

				// Load Config
				await using var configStream = await device.GetConfigurationAsync(ct);
				await using var configMemory = await GetNameFromConfig(configStream);

				Directory.CreateDirectory(Path.Combine(path, name));

				// Backup config
				await using var configFile = File.Create(Path.Combine(path, name, "config.json"));
				await configMemory.CopyToAsync(configFile, ct);

				// Load presets
				await using var presetsStream = await device.GetPresetsAsync(ct);

				// Backup presets
				await using var presetsFile = File.Create(Path.Combine(path, name, "presets.json"));
				await presetsStream.CopyToAsync(presetsFile, ct);

				log.LogInformation("Successfully backup config and presets of '{dev}'.", dev);

				async ValueTask<Stream> GetNameFromConfig(Stream networkStream)
				{
					var memory = new MemoryStream();
					await networkStream.CopyToAsync(memory, ct);

					memory.Position = 0;
					var config = await JsonSerializer.DeserializeAsync<Configuration>(memory, cancellationToken: ct);

					if (config?.Identity?.Name is { Length: > 0 } configName)
					{
						name = configName;
					}
					else
					{
						log.LogError("Failed to get name from configuration of device '{dev}'.", dev);
					}

					memory.Position = 0;
					return memory;
				}
			}
			catch (Exception error)
			{
				log.LogError(error, "Failed to backup device '{dev}'", dev);
				hc.SetFailed(error);
			}
		}
	}
}