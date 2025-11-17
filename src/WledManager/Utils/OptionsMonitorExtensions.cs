using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace WledManager.Utils;

internal static class OptionsMonitorExtensions
{
	public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IOptionsMonitor<T> options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		yield return options.CurrentValue;

		var next = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

		await using var registration = cancellationToken.Register(() => next.TrySetCanceled());
		using var subscription = options.OnChange(value => next.TrySetResult(value));

		while (!cancellationToken.IsCancellationRequested)
		{
			yield return await next.Task.ConfigureAwait(false);
			next = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
		}
	}
}