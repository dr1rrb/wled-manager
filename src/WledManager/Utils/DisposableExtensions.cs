namespace WledManager.Utils;

internal static class DisposableExtensions
{
	public static void DisposeWith(this IDisposable disposable, CancellationToken ct)
		=> ct.Register(disposable.Dispose);
}