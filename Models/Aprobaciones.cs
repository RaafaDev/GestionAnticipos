namespace GestionAnticiposApp.Models
{
    public class Aprobaciones
    {
        public int Id { get; set; }
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }

        public int ProcesoVinculadoId { get; set; }
        public ProcesosVinculados ProcesoVinculado { get; set; }
    }
}
