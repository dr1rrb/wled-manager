namespace WledManager.Backups;

/// <summary>
/// Options to configure the <see cref="BackupService"/>.
/// </summary>
public class BackupOptions
{
	/// <summary>
	/// Path to store backups in.
	/// </summary>
	public string Path { get; set; } = "/wled-manager/backups";

	/// <summary>
	/// The time of day to perform the backup.
	/// </summary>
	public TimeSpan TimeOfDay { get; set; } = TimeSpan.FromHours(3);

	/// <summary>
	/// The delay to wait on start before starting the first backup.
	/// </summary>
	/// <remarks>Set to `null` to disable backup on start.</remarks>
	public TimeSpan? StartDelay { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// List of hostnames or IP addresses of devices to back up.
	/// </summary>
	public List<string> Devices { get; set; } = new();

	/// <summary>
	/// An optional health check URL to ping after a successful backup.
	/// </summary>
	public Uri? Health { get; set; }
}