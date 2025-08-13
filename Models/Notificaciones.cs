namespace GestionAnticiposApp.Models
{
    public class Notificaciones
    {
        public int Id { get; set; }
        public string Origen { get; set; }
        public string Tipo { get; set; }
        public string Mensaje { get; set; }

        public int ProcesoVinculadoId { get; set; }
        public ProcesosVinculados ProcesoVinculado { get; set; }
    }
}
