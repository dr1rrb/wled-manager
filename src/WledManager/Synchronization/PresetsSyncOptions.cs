namespace WledManager.Synchronization;

public class PresetsSyncOptions
{
	public string Source { get; set; }

	public List<string> Targets { get; set; } = new();
}