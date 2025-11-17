namespace WledManager.Synchronization;

/// <summary>
/// Defines operations that synchronize presets across devices.
/// </summary>
public interface IPresetsSyncService
{
	/// <summary>
	/// Copies presets from every configured source device to its targets.
	/// </summary>
	/// <param name="ct">Token used to cancel the synchronization.</param>
	Task SyncAsync(CancellationToken ct);
}