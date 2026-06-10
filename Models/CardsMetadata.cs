using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    [MetadataType(typeof(CardsMetadata))]
    public partial class Cards
    {
    }

    public class CardsMetadata
    {
        [Required(ErrorMessage = "El día de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string card_payday { get; set; }

        [Required(ErrorMessage = "El monto de pago es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ser mayor que cero.")]
        public int card_payment_amount { get; set; }

        [Required(ErrorMessage = "El precio del artículo es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ser mayor que cero.")]
        public int card_item_price { get; set; }

        [Required(ErrorMessage = "El nombre del artículo es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string card_item { get; set; }
    }
}