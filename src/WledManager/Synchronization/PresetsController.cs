using Microsoft.AspNetCore.Mvc;

namespace WledManager.Synchronization;

/// <summary>
/// HTTP endpoint for launching preset synchronization runs.
/// </summary>
/// <param name="service">Service that performs the synchronization work.</param>
[ApiController]
[Route("api/presets")]
public class PresetsController(IPresetsSyncService service)
{
	/// <summary>
	/// Synchronizes presets across configured WLED devices.
	/// </summary>
	/// <remarks>
	/// Synchronizes presets from source WLED devices to target devices based on configuration.
	/// This ensures all devices have the same preset configuration.
	/// </remarks>
	/// <param name="ct">Cancellation token</param>
	/// <response code="200">Synchronization completed successfully</response>
	/// <response code="500">An error occurred during synchronization</response>
	[HttpPost("sync")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async ValueTask RunAsync(CancellationToken ct)
		=> await service.SyncAsync(ct);
}