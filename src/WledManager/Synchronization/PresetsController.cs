using Microsoft.AspNetCore.Mvc;

namespace WledManager.Synchronization;

[ApiController]
[Route("presets")]
public class PresetsController(PresetsSyncService service)
{
	[HttpPost(Name = "sync")]
	public async ValueTask RunAsync(CancellationToken ct)
		=> await service.SyncAsync(ct);
}