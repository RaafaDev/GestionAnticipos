namespace GestionAnticiposApp.Models
{
    public class Documentos
    {
            
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Archivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }

        public int ProcesoVinculadoId { get; set; }
        public ProcesosVinculados ProcesoVinculado { get; set; }
    
    }
}
