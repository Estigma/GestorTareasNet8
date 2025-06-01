namespace TaskService.DTOs
{
    public class UsuarioValidationRequest
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public int UsuarioId { get; set; }
    }

    public class UsuarioValidationResponse
    {
        public Guid RequestId { get; set; }
        public bool Existe { get; set; }
        public string? Error { get; set; }
    }
}
