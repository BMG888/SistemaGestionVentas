using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Models.ViewModels;
using SistemaGestionVentas.Helpers;
using System.Text.RegularExpressions;

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
        [ValidateAntiForgeryToken] // protege contra ataques Cross Site Request Forgery generando un token oculto para que el servidor lo valide
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var user = db.Users.FirstOrDefault(u => u.user_email == model.user_email); // se crea una variable a partir del email digitado por el usuario con el de la base de datos comparando que sean iguales

                if (user == null) // si no existe
                {
                    ViewBag.Error = "Usuario no encontrado o no existe.";
                    return View(model);
                }

                if (!user.user_active)
                {
                    ViewBag.Error = "Usuario no disponible.";
                    return View(model);
                }                

                string hashedPassword = PasswordHelper.HashPassword(model.user_password);

                if (user.user_password != hashedPassword)
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

        public ActionResult Logout()
        {
            try
            {
                Session.Clear(); // se eliminan las variables almacenadas
                Session.Abandon();
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                TempData["Error"] = "No se pudo cerrar la sesión."; 
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult ChangePassword()
        {
            if (Session["UserId"] == null) 
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // protege contra ataques Cross Site Request Forgery generando un token oculto para que el servidor lo valide
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Login");
                }

                int userId = Convert.ToInt32(Session["UserId"]);
                var user = db.Users.FirstOrDefault(u => u.user_id == userId);

                if (user == null || !user.user_active)
                {
                    ViewBag.Error = "Usuario no disponible.";
                    return View(model);
                }

                string currentHash = PasswordHelper.HashPassword(model.current_password);

                if (user.user_password != currentHash)
                {
                    ModelState.AddModelError("current_password", "La contraseña actual es incorrecta.");
                    return View(model);
                }

                if (model.new_password != model.confirm_password)
                {
                    ModelState.AddModelError("confirm_password", "Las contraseñas no coinciden.");
                    return View(model);
                }

                if (model.new_password.Length < 8)
                {
                    ModelState.AddModelError("new_password", "La contraseña debe tener mínimo 8 caracteres.");                    
                    return View(model);
                }

                if (!Regex.IsMatch(model.new_password, "[A-Z]")) // regex busca patrones dentro de un texto
                {
                    ModelState.AddModelError("new_password", "La contraseña debe contener al menos una letra mayúscula.");                    
                    return View(model);
                }

                if (!Regex.IsMatch(model.new_password, "[a-z]"))
                {
                    ModelState.AddModelError("new_password", "La contraseña debe contener al menos una letra minúscula.");                    
                    return View(model);
                }

                if (!Regex.IsMatch(model.new_password, "[0-9]"))
                {
                    ModelState.AddModelError("new_password", "La contraseña debe contener al menos un número.");                    
                    return View(model);
                }

                if (!Regex.IsMatch(model.new_password, @"[\W_]"))
                {
                    ModelState.AddModelError("new_password", "La contraseña debe contener al menos un símbolo.");                    
                    return View(model);
                }

                string newHash = PasswordHelper.HashPassword(model.new_password);

                if (newHash == user.user_password)
                {
                    ModelState.AddModelError("new_password", "La nueva contraseña debe ser diferente.");                    
                    return View(model);
                }
                user.user_password = newHash;
                db.SaveChanges();
                TempData["Success"] = "Contraseña actualizada correctamente.";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewBag.Error = "No se pudo actualizar la contraseña.";
                return View(model);
            }
        }
    }
}