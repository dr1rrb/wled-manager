using Microsoft.AspNetCore.Mvc;

namespace WledManager.Backups;

[ApiController]
[Route("backups")]
public class BackupController(IBackupService service)
{
	[HttpPost(Name = "run")]
	public async ValueTask RunAsync(CancellationToken ct)
		=> await service.BackupAsync(ct);
}