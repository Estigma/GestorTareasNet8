namespace TaskService.Interfaces
{
    public interface IUsuarioValidator
    {
        void Dispose();
        Task<bool> UsuarioExisteAsync(int usuarioId);
    }
}