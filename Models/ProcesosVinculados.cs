using System.Diagnostics.Contracts;

namespace GestionAnticiposApp.Models
{

    public enum TipoProcesoVinculado
    {
        Anticipo = 0,
        Tiquete = 1,
        Legalizacion = 2
    }
    public class ProcesosVinculados
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Estado { get; set; }
        public string Funcionario { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string? Autorizador { get; set; }
        public TipoProcesoVinculado Tipo { get; set; }
        public decimal Valor { get; set; }

        public int ContratoId { get; set; }
        public Contratos Contrato { get; set; }

        public ICollection<Notificaciones> Notificaciones { get; set; }
        public ICollection<Aprobaciones> Aprobaciones { get; set; }
        public ICollection<Documentos> Documentos { get; set; }
    }
}
