using GestionAnticiposApp.Models;

namespace GestionAnticiposApp.ViewModels
{
    public class TiquetesVM
    {
        public int Id { get; set; }
        public int ProcesoVinculadoId { get; set; }
        public int ContratoId { get; set; }
        public string Concepto { get; set; }
        public decimal Valor { get; set; }
        public DateTime Fecha { get; set; }
        public string? Estado { get; set; }

        // Para subir archivos en el formulario
        public List<IFormFile> Archivos { get; set; }

        // Para mostrar archivos ya guardados
        public List<string> Documentos { get; set; } = new List<string>();
    }
}
