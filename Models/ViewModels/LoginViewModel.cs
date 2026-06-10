using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "Máximo 150 caracteres.")]
        public string user_email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string user_password { get; set; }
    }
}