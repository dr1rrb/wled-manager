namespace WledManager.Synchronization;

public interface IPresetsSyncService
{
	Task SyncAsync(CancellationToken ct);
}