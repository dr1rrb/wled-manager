namespace WledManager.Utils.HealthChecks;

public class HealthChecksService(ILogger<HealthChecksService> log) : IHealthChecksService
{
	public IHealthCheck StartNew(Uri? checkUrl)
		=> checkUrl is null
			? NullCheck.Instance
			: new HealthCheck(checkUrl, log);

	private class HealthCheck : IHealthCheck
	{
		private readonly CancellationTokenSource _startToken = new();
		private readonly Uri _checkUrl;
		private readonly ILogger<HealthChecksService> _log;
		private readonly HttpClient _client;
		private readonly Task _start;
		private Task? _completion;

		public HealthCheck(Uri checkUrl, ILogger<HealthChecksService> log)
		{
			if (!checkUrl.OriginalString.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				checkUrl = new Uri(checkUrl.OriginalString + "/");
			}

			_checkUrl = checkUrl;
			_log = log;

			_client = new() { BaseAddress = checkUrl, Timeout = TimeSpan.FromSeconds(5) };
			_start = Send("start", _startToken.Token);
		}

		public void SetFailed(Exception? error = null)
		{
			var tcs = new TaskCompletionSource();
			if (Interlocked.CompareExchange(ref _completion, tcs.Task, null) is not null)
			{
				throw new InvalidOperationException($"Check '{_checkUrl}' has already been completed");
			}

			_ = Task.Run(async () =>
			{
				try
				{
					await _startToken.CancelAsync();
					await _start; // Make sure the start signal has been successfully sent (or cancelled) before notifying completion

					await Send("fail", CancellationToken.None);
				}
				finally
				{
					tcs.TrySetResult();
				}
			});
		}

		private async Task Send(string? action, CancellationToken ct)
		{
			_log.LogTrace("Sending {action} on check '{url}'.", action ?? "ping", _checkUrl);
			try
			{
				var resp = await _client.GetAsync(action, ct);
				resp.EnsureSuccessStatusCode();

				_log.LogDebug("Successfully sent {action} on check '{url}'.", action ?? "ping", _checkUrl);
			}
			catch (OperationCanceledException) when (ct.IsCancellationRequested)
			{
			}
			catch (Exception ex)
			{
				_log.LogError(ex, "Failed to send {action} on check '{url}'.", action ?? "ping", _checkUrl);
			}
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			await _startToken.CancelAsync();
			await _start; // Make sure the start signal has been successfully sent (or cancelled) before notifying completion

			if (Interlocked.CompareExchange(ref _completion, Task.CompletedTask, null) is null)
			{
				// Success
				await Send(null, CancellationToken.None);
			}
			else
			{
				// Error (or already disposed - if so, we cannot await the completion as we put a Task.CompletedTask)
				await _completion;
			}

			_client.Dispose();
		}
	}

	private class NullCheck : IHealthCheck
	{
		public static IHealthCheck Instance { get; } = new NullCheck();

		/// <inheritdoc />
		public void SetFailed(Exception? error = null)
		{
		}


		/// <inheritdoc />
		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}

}