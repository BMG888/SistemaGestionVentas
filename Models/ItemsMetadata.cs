using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    [MetadataType(typeof(ItemsMetadata))]
    public partial class Items
    {
    }

    public class ItemsMetadata
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string item_name { get; set; }
        
        public string item_url { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(255, ErrorMessage = "Máximo 255 caracteres.")]
        public string item_description { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un álbum.")]
        public int album_id { get; set; }
    }
}