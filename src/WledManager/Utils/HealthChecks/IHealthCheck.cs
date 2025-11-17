namespace WledManager.Utils.HealthChecks;

public interface IHealthCheck : IAsyncDisposable
{
	void SetFailed(Exception? error = null);
}