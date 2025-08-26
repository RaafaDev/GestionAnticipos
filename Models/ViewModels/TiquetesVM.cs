using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionAnticiposApp.ViewModels
{
    public class TiquetesVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El anticipo es obligatorio")]
        public int ProcesoVinculadoId { get; set; }   // Anticipo al que pertenece

        [Required(ErrorMessage = "Debe ingresar un concepto")]
        [StringLength(200)]
        public string Concepto { get; set; }

        [Required(ErrorMessage = "Debe ingresar el valor")]
        [Range(1, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Pendiente";

        // Archivos adjuntos
        public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();

        // Para mostrar los nombres de los documentos ya cargados
        public List<string> Documentos { get; set; } = new List<string>();

        // Para mostrar info del anticipo en la vista
        public string AnticipoCodigo { get; set; }
        public string ContratoCodigo { get; set; }
    }
}







