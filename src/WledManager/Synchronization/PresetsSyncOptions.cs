namespace WledManager.Synchronization;

/// <summary>
/// Options to configure the presets synchronization.
/// </summary>
public class PresetsSyncOptions
{
	/// <summary>
	/// Hostname or IP address of the source device.
	/// </summary>
	public required string Source { get; set; }

	/// <summary>
	/// Hostnames or IP addresses of target devices.
	/// </summary>
	public List<string> Targets { get; set; } = new();
}