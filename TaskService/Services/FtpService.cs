using System.Net;
using TaskService.Interfaces;

namespace TaskService.Services
{
    public class FtpService : IFtpService
    {
        private readonly string _ftpHost;
        private readonly string _ftpUser;
        private readonly string _ftpPassword;

        public FtpService(IConfiguration config)
        {
            _ftpHost = config["Ftp:Host"] ?? "";
            _ftpUser = config["Ftp:Username"] ?? "";
            _ftpPassword = config["Ftp:Password"] ?? "";
        }

        public async Task<string> DescargarArchivoAsync(string nombreArchivo)
        {
            var requestUri = new Uri($"ftp://{_ftpHost}/{nombreArchivo}");

            var request = (FtpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }
    }
}
