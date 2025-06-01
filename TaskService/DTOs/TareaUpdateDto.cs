using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs
{
    public class TareaUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de la tarea es obligatorio.")]
        public string CodigoTarea { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción de la tarea es obligatoria.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Los criterios de aceptación de la tarea son obligatorios.")]
        public string CriteriosAceptacion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public int TiempoDesarrollo { get; set; }

        [Required(ErrorMessage = "El estado de la tarea es obligatorio.")]
        [RegularExpression("^(Backlog|Doing|InReview|Done)$", ErrorMessage = "El estado no es válido. Use: Backlog, Doing, InReview, Done.")]
        public string EstadoTarea { get; set; }

        public bool Estado { get; set; } = true;

        public int UsuarioId { get; set; }
    }
}
