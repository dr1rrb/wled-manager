using System.Net.Http.Headers;

namespace WledManager;

public class WledClient(Uri host) : IDisposable
{
	public WledClient(string host)
		: this(Uri.TryCreate(host, UriKind.Absolute, out var uri) ? uri : new Uri("http://" + host))
	{
	}

	private HttpClient? _client;
	private HttpClient Client => _client ??= new HttpClient { BaseAddress = host, Timeout = TimeSpan.FromSeconds(15) };

	public async ValueTask<Stream> GetConfigurationAsync(CancellationToken ct)
		=> await Client.GetStreamAsync("cfg.json", ct);

	public async ValueTask<Stream> GetPresetsAsync(CancellationToken ct)
		=> await Client.GetStreamAsync("presets.json", ct);

	public async ValueTask SetPresetsAsync(string json, CancellationToken ct)
	{
		/*
*

------geckoformboundary5995db199860ac3110d0b49707246d2
	Content-Disposition: form-data; name="data"; filename="/presets.json"
	Content-Type: application/json

	{
}

	------geckoformboundary5995db199860ac3110d0b49707246d2--

*
*/
		var part = new StringContent(json)
		{
			Headers =
			{
				ContentDisposition = new ContentDispositionHeaderValue("form-data")
				{
					Name = "data",
					FileName = "/presets.json"
				},
				ContentType = new MediaTypeHeaderValue("application/json")
			}
		};

		var response = await Client.PostAsync("upload", new MultipartFormDataContent { part }, ct);
		response.EnsureSuccessStatusCode();
	}

	/// <inheritdoc />
	public void Dispose()
		=> _client?.Dispose();

	
}