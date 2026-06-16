using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    [MetadataType(typeof(CollectionsMetadata))]
    public partial class Collections
    {
    }
    public class CollectionsMetadata
    {
        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a cero, no puede ser negativo.")]
        public int collection_amount { get; set; }

        [StringLength(255, ErrorMessage = "La nota no puede superar los 255 caracteres.")]
        public string collection_note { get; set; }
    }
}