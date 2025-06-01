namespace TaskService.DTOs
{
    public class AsignacionTareaMessage
    {
        public int TareaId { get; set; }
        public int UsuarioId { get; set; }
        public string TituloTarea { get; set; }
    }
}