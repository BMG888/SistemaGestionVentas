using SistemaGestionVentas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaGestionVentas.Controllers
{    
    public class HomeController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        public ActionResult Index()
        {
            var random = new Random();
            var carouselItems = db.Items.Where(i => i.item_active).ToList().OrderBy(i => random.Next()).Take(8).ToList();
            ViewBag.CarouselItems = carouselItems;
            return View();
        }        
    }
}