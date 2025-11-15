namespace WledManager.Backups;

public class BackupOptions
{
	public string Path { get; set; } = "/wled-manager/backups";

	public TimeSpan TimeOfDay { get; set; } = TimeSpan.FromHours(3);

	/// <summary>
	/// The delay to wait on start before starting the first backup.
	/// </summary>
	/// <remarks>Set to `null` to disable backup on start.</remarks>
	public TimeSpan? StartDelay { get; set; } = TimeSpan.FromSeconds(30);

	public List<string> Devices { get; set; } = new();

	public Uri? Health { get; set; }
}