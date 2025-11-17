namespace WledManager.Backups;

/// <summary>
/// Exposes operations for backing up configured WLED devices.
/// </summary>
public interface IBackupService
{
	/// <summary>
	/// Executes a backup run for every configured device.
	/// </summary>
	/// <param name="ct">Token used to cancel the backup operation.</param>
	Task BackupAsync(CancellationToken ct);
}