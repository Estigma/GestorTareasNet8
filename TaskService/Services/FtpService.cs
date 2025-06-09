using System.Net;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Polly.Retry;
using TaskService.Interfaces;

namespace TaskService.Services
{
    public class FtpService : IFtpService
    {
        private readonly string _ftpHost;
        private readonly string _ftpUser;
        private readonly string _ftpPassword;
        private readonly AsyncRetryPolicy<string> _retryPolicy;

        public FtpService(IConfiguration config, AsyncRetryPolicy<string> retryPolicy)
        {
            _ftpHost = config["Ftp:Host"] ?? "";
            _ftpUser = config["Ftp:Username"] ?? "";
            _ftpPassword = config["Ftp:Password"] ?? "";
            _retryPolicy = retryPolicy;
        }

        public async Task<string> DescargarArchivoAsync(string nombreArchivo)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var requestUri = new Uri($"ftp://{_ftpHost}/{nombreArchivo}");
                var request = (FtpWebRequest)WebRequest.Create(requestUri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream);

                return await reader.ReadToEndAsync();
            });
        }
    }
}
