using TaskService.DTOs;
using TaskService.Models;

namespace TaskService.Data
{
    public interface ITareaRepository
    {
        Task<IEnumerable<Tarea>> GetAllTareasAsync();
        Task<Tarea> GetTareaByIdAsync(int id);
        Task CreateTareaAsync(Tarea tarea);
        Task UpdateTareaAsync(Tarea tarea);
        Task DeleteTareaAsync(int id);
        Task<IEnumerable<Tarea>> GetTareasByUsuarioAsync(int usuarioId);
        Task<int> ImportarTareasAsync(List<TareaImportada> tareasImportadas);
    }
}