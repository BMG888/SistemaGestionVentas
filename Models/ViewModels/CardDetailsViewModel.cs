using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace SistemaGestionVentas.Models.ViewModels
{
    public class CardDetailsViewModel
    {
        public Cards Card { get; set; }

        public IPagedList<Collections> Collections { get; set; }

        public DateTime? CollectionDateFilter { get; set; }

        public int? UserIdFilter { get; set; }

        public bool? CollectionActiveFilter { get; set; }
    }
}