namespace GestionAnticiposApp.Models
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Nivel { get; set; }
        public string Mensaje { get; set; }
        public string? Entidad { get; set; }
        public string? Campo { get; set; }
        public string? ValorAntes { get; set; }
        public string? ValorDespues { get; set; }
    }
}
