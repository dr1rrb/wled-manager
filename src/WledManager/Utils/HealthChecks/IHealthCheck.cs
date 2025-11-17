namespace WledManager.Utils.HealthChecks;

/// <summary>
/// Represents a disposable health-check scope that reports the outcome of an operation.
/// </summary>
public interface IHealthCheck : IAsyncDisposable
{
	/// <summary>
	/// Marks the health check as failed and optionally associates the underlying exception.
	/// </summary>
	/// <param name="error">Optional failure details.</param>
	void SetFailed(Exception? error = null);
}