namespace TaskService.Interfaces
{
    public interface IFtpService
    {
        Task<string> DescargarArchivoAsync(string nombreArchivo);
    }
}