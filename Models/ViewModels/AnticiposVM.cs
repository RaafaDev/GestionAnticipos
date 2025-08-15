namespace GestionAnticiposApp.Models.ViewModels
{
    public class AnticipoVM
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Estado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public decimal Valor { get; set; }

        // Campos adicionales solo para la vista
        public string Comentarios { get; set; }
        public bool PuedeAprobar { get; set; }
    }
}