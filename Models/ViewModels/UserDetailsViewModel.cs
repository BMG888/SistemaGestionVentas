using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace SistemaGestionVentas.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        public Users User { get; set; }

        public IPagedList<Cards> Cards { get; set; }        

        public string CardPaydayFilter { get; set; }

        public int? FrequencyIdFilter { get; set; }

        public bool? CardStateFilter { get; set; }

        public bool? CardActiveFilter { get; set; }
    }
}