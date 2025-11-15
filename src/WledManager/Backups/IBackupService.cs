namespace WledManager.Backups;

public interface IBackupService
{
	Task BackupAsync(CancellationToken ct);
}