using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    [MetadataType(typeof(AlbumsMetadata))]
    public partial class Albums 
    { 
    }

    public class AlbumsMetadata
    {
        [Required(ErrorMessage = "El nombre del álbum es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string album_name { get; set; }
    }
}