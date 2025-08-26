namespace GestionAnticiposApp.Models
{
    public class Documentos
    {
            
        public int Id { get; set; }
    
        public string Archivo { get; set; }

        public int ProcesoVinculadoId { get; set; }
        public ProcesosVinculados ProcesoVinculado { get; set; }
    
    }
}
