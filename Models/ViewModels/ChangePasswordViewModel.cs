using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public string current_password { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        public string new_password { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        public string confirm_password { get; set; }
    }
}