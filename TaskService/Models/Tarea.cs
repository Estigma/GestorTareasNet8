using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskService.Models
{
    public enum EstadoTarea
    {
        Backlog,
        Doing,
        InReview,
        Done
    }

    public class Tarea
    {
        public int Id { get; set; }        
        public string CodigoTarea { get; set; }        
        public string Titulo { get; set; }        
        public string Descripcion { get; set; }        
        public string CriteriosAceptacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public int TiempoDesarrollo { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EstadoTarea EstadoTarea { get; set; }
        public bool Estado { get; set; } = true;
        public int UsuarioId { get; set; }
    }
}