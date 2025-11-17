using Microsoft.AspNetCore.Mvc;

namespace WledManager.Backups;

/// <summary>
/// HTTP surface for triggering manual backup runs.
/// </summary>
/// <param name="service">Service responsible for performing backups.</param>
[ApiController]
[Route("api/backups")]
public class BackupController(IBackupService service)
{
	/// <summary>
	/// Triggers a manual backup of all WLED devices.
	/// </summary>
	/// <remarks>
	/// Performs an immediate backup of presets, configuration, and state for all configured WLED devices.
	/// This operation runs asynchronously and may take several seconds depending on the number of devices.
	/// </remarks>
	/// <param name="ct">Cancellation token</param>
	/// <response code="200">Backup completed successfully</response>
	/// <response code="500">An error occurred during backup</response>
	[HttpPost("run")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async ValueTask RunAsync(CancellationToken ct)
		=> await service.BackupAsync(ct);
}