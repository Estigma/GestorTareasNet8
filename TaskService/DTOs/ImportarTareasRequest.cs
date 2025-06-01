using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs
{
    public class ImportarTareasRequest
    {
        [Required]
        public string NombreArchivo { get; set; }
    }

    public class TareaImportada
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        public string CodigoTarea { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Los criterios son obligatorios")]
        public string CriteriosAceptacion { get; set; }
    }
}
