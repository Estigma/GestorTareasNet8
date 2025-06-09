using Polly;
using Polly.Retry;
using System;
using System.Net;

namespace TaskService.Policies
{
    public static class PollyPolicies
    {
        public static AsyncRetryPolicy<string> GetFtpRetryPolicy(ILogger logger)
        {
            return Policy<string>
                .Handle<WebException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning(exception.Exception,
                            "[Polly] Error de FTP detectado. Reintentando en {TimeSpan} segundos... (Intento {RetryCount})",
                            timeSpan.TotalSeconds, retryCount);
                    }
                );
        }
    }
}
