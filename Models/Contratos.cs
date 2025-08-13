using System.ComponentModel.DataAnnotations;

namespace GestionAnticiposApp.Models
{
    public class Contratos
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Estado { get; set; }
        public string Servicio { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaInicio { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaFin { get; set; }
        public string Empresa { get; set; }
        public string Referencia { get; set; }


    }
}
