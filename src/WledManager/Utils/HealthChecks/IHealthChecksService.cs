namespace WledManager.Utils.HealthChecks;

public interface IHealthChecksService
{
	IHealthCheck StartNew(Uri? checkUrl);
}