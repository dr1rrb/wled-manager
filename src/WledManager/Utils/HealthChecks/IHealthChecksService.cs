namespace WledManager.Utils.HealthChecks;

/// <summary>
/// Factory that creates <see cref="IHealthCheck"/> scopes for a given monitoring endpoint.
/// </summary>
public interface IHealthChecksService
{
	/// <summary>
	/// Starts a new health-check scope targeting the provided URL (if any).
	/// </summary>
	/// <param name="checkUrl">Optional endpoint that should receive status pings.</param>
	/// <returns>A disposable scope representing the health-check run.</returns>
	IHealthCheck StartNew(Uri? checkUrl);
}