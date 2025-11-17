using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using WledManager.Backups;
using WledManager.DTO;
using WledManager.Utils;

namespace WledManager.Synchronization;

/// <summary>
/// Background worker that propagates presets from a canonical WLED device to multiple targets.
/// </summary>
public class PresetsSyncService(IOptionsMonitor<List<PresetsSyncOptions>> options, ILogger<BackupService> log) : BackgroundService, IPresetsSyncService
{
	private static readonly JsonSerializerOptions _noEscapeJson = new(JsonSerializerOptions.Default)
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await SyncAsync(stoppingToken);

		options
			.OnChange(opts => _ = SyncAsync(stoppingToken))
			?.DisposeWith(stoppingToken);
	}

	/// <inheritdoc />
	public async Task SyncAsync(CancellationToken ct)
	{
		log.LogTrace("Starting sync of WLED presets.");

		foreach (var opt in options.CurrentValue)
		{
			log.LogTrace("Starting sync presets from '{src}' to '{dst}'.", opt.Source, string.Join("', '", opt.Targets));

			using var src = new WledClient(opt.Source);
			try
			{
				var presetsStream = await src.GetPresetsAsync(ct);
				var presets = await JsonNode.ParseAsync(presetsStream, cancellationToken: ct)
					?? throw new InvalidOperationException("Failed to parse presets.");

				foreach (var target in opt.Targets)
				{
					try
					{
						using var dst = new WledClient(target);

						await using var configStream = await dst.GetConfigurationAsync(ct);
						var config = await JsonSerializer.DeserializeAsync<Configuration>(configStream, cancellationToken: ct);
						var ledCount = config?.Hardware?.Leds?.Total ?? 512;

						PatchPresets(ref presets, ledCount);

						var presetsJson = presets.ToJsonString(_noEscapeJson);

						await dst.SetPresetsAsync(presetsJson, ct);

						log.LogInformation("Successfully sync presets from '{src}' of '{dst}'.", opt.Source, target);
					}
					catch (Exception error)
					{
						log.LogError(error, "Failed to sync presets from '{src}' of '{dst}'.", opt.Source, target);
					}
				}
			}
			catch (Exception error)
			{
				log.LogError(error, "Failed to load/sync presets from '{src}'.", opt.Source);
			}
		}
	}

	private void PatchPresets(ref JsonNode presets, int ledCount)
	{
		// Search for root > "1" (preset id) > "seg" (segments) > [ ... ] (array of segments) > "stop"
		// and update "stop" property from each segment to match the number of LED of the strip.
		foreach (var preset in presets.AsObject())
		{
			if ((preset.Value?.AsObject().TryGetPropertyValue("seg", out var segmentsNode) ?? false)
				&& segmentsNode is JsonArray segments)
			{
				foreach (var segment in segments.OfType<JsonObject>())
				{
					if (segment.TryGetPropertyValue("stop", out var stopNode)
						&& stopNode is JsonValue stop
						&& stop.TryGetValue(out int stopValue)
						&& stopValue is not 0)
					{
						stop.ReplaceWith(ledCount);
					}
				}
			}
		}
	}
}