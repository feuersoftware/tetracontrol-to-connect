using System.Reactive.Linq;

namespace FeuerSoftware.TetraControl2Connect.Extensions
{
    public static class ObservableExtensions
    {
        public static IDisposable SubscribeAsyncSafe<T>(
            this IObservable<T> source,
            Func<T, Task> onNextAsync,
            Action<Exception> onError,
            Action onCompleted)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(onNextAsync);
            ArgumentNullException.ThrowIfNull(onError);
            ArgumentNullException.ThrowIfNull(onCompleted);

            return source
                .Select(arg => Observable.FromAsync(async () =>
                {
                    try
                    {
                        await onNextAsync(arg).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        onError(ex);
                    }
                }))
                .Concat()
                .Subscribe(_ => { }, onError, onCompleted);
        }
    }
}
