using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace SistemaGestionVentas.Models.ViewModels
{
    public class AlbumDetailsViewModel
    {
        public Albums Album { get; set; }

        public IPagedList<Items> Items { get; set; }

        public string ItemNameFilter { get; set; }

        public bool? ItemActiveFilter { get; set; }
    }
}