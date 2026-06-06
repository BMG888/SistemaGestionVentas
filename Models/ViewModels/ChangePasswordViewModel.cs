using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaGestionVentas.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string current_password { get; set; }

        public string new_password { get; set; }

        public string confirm_password { get; set; }
    }
}