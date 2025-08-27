using System.ComponentModel.DataAnnotations;

namespace GestionAnticiposApp.Models.ViewModels
{
    public class AnticipoVM
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        [Required]
        [RegularExpression("^(Pendiente|Aprobado|Rechazado)$", ErrorMessage = "Estado inválido.")]
        public string Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public decimal Valor { get; set; }
        public string? Funcionario { get; set; }

        public string? Autorizador { get; set; }

        public List<Documentos> Documentos { get; set; } = new();



        // Campos adicionales solo para la vista
        public string Comentarios { get; set; }
        public bool PuedeAprobar { get; set; }
    }
}