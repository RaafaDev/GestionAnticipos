namespace GestionAnticiposApp.Models
{
    public class Contratos
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Estado { get; set; }
        public string Servicio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Empresa { get; set; }
        public string Referencia { get; set; }

        public ICollection<ProcesosVinculados> ProcesosVinculados { get; set; }
    }
}
