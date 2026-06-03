using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Models.ViewModels;

namespace SistemaGestionVentas.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        private SistemaGestionVentasDBEntities db =
            new SistemaGestionVentasDBEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                var user = db.Users.FirstOrDefault(u =>
                    u.user_email == model.user_email);

                if (user == null)
                {
                    ViewBag.Error = "Usuario no encontrado o no existe.";
                    return View(model);
                }

                if (!user.user_active)
                {
                    ViewBag.Error = "Usuario no disponible.";
                    return View(model);
                }

                if (user.user_password != model.user_password)
                {
                    ViewBag.Error = "Credenciales inválidas.";
                    return View(model);
                }

                Session["UserId"] = user.user_id;
                Session["UserName"] = user.user_name;
                Session["RoleId"] = user.role_id;

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewBag.Error = "No se pudo iniciar sesión.";
                return View(model);
            }
        }
    }
}