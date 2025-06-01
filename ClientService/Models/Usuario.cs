namespace ClientService.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Identificacion { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public int Edad { get; set; }
        public string Cargo { get; set; }
        public bool Estado { get; set; } = true;
    }
}