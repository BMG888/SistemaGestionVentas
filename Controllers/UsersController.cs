using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Filters;
using SistemaGestionVentas.Helpers;
using System.Text.RegularExpressions;
using System.Globalization;
using PagedList;
using SistemaGestionVentas.Models.ViewModels;

namespace SistemaGestionVentas.Controllers
{
    [SessionAuthorize]
    public class UsersController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        // GET: Users
        public ActionResult Index(string user_name, string user_lastname, string user_nickname, bool? user_active, int? role_id, int? page)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId == 3)
            {
                return RedirectToAction("Index", "Home");
            }

            var users = db.Users.Include(u => u.Roles).AsQueryable();

            if (roleId == 2)
            {
                users = users.Where(u => u.user_active);
            }

            if (!string.IsNullOrWhiteSpace(user_name))
            {
                users = users.Where(u => u.user_name.Contains(user_name));
            }

            if (!string.IsNullOrWhiteSpace(user_lastname))
            {
                users = users.Where(u => u.user_lastname.Contains(user_lastname));
            }

            if (!string.IsNullOrWhiteSpace(user_nickname))
            {
                users = users.Where(u => u.user_nickname.Contains(user_nickname));
            }

            if (user_active.HasValue)
            {
                users = users.Where(u => u.user_active == user_active.Value);
            }

            if (role_id.HasValue)
            {
                users = users.Where(u => u.role_id == role_id.Value);
            }

            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description");

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(users.OrderBy(u => u.user_name).ToPagedList(pageNumber, pageSize));
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id, string card_payday, int? frequency_id, bool? card_state, bool? card_active, int page = 1)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                int roleId = Convert.ToInt32(Session["RoleId"]);
                int userId = Convert.ToInt32(Session["UserId"]);

                if (roleId == 3 && id != userId)
                {
                    return RedirectToAction("Details", new { id = userId });
                }

                Users users = db.Users.Find(id);

                if (users == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                var cardsQuery = db.Cards.Include(c => c.Frequencies).Where(c => c.user_id == users.user_id);

                if (!string.IsNullOrWhiteSpace(card_payday))
                {
                    cardsQuery = cardsQuery.Where(c => c.card_payday.Contains(card_payday));
                }

                if (frequency_id.HasValue)
                {
                    cardsQuery = cardsQuery.Where(c => c.frequency_id == frequency_id.Value);
                }

                if (card_state.HasValue)
                {
                    cardsQuery = cardsQuery.Where(c => c.card_state == card_state.Value);
                }

                if (roleId == 3)
                {
                    cardsQuery = cardsQuery.Where(c => c.card_active);
                }
                else
                {
                    if (card_active.HasValue)
                    {
                        cardsQuery = cardsQuery.Where(c => c.card_active == card_active.Value);
                    }
                }

                int pageSize = 5;
                var cards = cardsQuery.OrderBy(c => c.card_id).ToPagedList(page, pageSize);

                var viewModel = new UserDetailsViewModel
                {
                    User = users,
                    Cards = cards,                    

                    CardPaydayFilter = card_payday,
                    FrequencyIdFilter = frequency_id,
                    CardStateFilter = card_state,
                    CardActiveFilter = card_active
                };

                ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", frequency_id);
                return View(viewModel);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index");
            }
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description");
            return View();
        }

        // POST: Users/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "user_id,user_name,user_lastname,user_nickname,user_phone,user_email,user_password,role_id")] Users users)
        {
            int actorRole = Convert.ToInt32(Session["RoleId"]);

            if (ModelState.IsValid)
            {                
                try
                {
                    string email = users.user_email?.Trim(); // elimina los espacios en blanco                    
                    bool emailExists = db.Users.Any(u => u.user_email.ToLower() == email.ToLower()); // busca si existe el correo, comparándolo en minúsculas

                    if (emailExists)
                    {
                        ModelState.AddModelError("user_email", "Este correo ya está en uso.");
                        if (actorRole == 1)
                        {
                            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                        }
                        return View(users);
                    }

                    if (users.user_password.Length < 8)
                    {
                        ModelState.AddModelError("user_password", "La contraseña debe tener mínimo 8 caracteres.");                        
                        if (actorRole == 1)
                        {
                            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                        }
                        return View(users);
                    }

                    if (!Regex.IsMatch(users.user_password, "[A-Z]"))
                    {
                        ModelState.AddModelError("user_password", "La contraseña debe contener al menos una letra mayúscula.");                        
                        if (actorRole == 1)
                        {
                            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                        }
                        return View(users);
                    }

                    if (!Regex.IsMatch(users.user_password, "[a-z]"))
                    {
                        ModelState.AddModelError("user_password", "La contraseña debe contener al menos una letra minúscula.");                        
                        if (actorRole == 1)
                        {
                            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                        }
                        return View(users);
                    }

                    if (!Regex.IsMatch(users.user_password, "[0-9]"))
                    {
                        ModelState.AddModelError("user_password", "La contraseña debe contener al menos un número.");                        
                        if (actorRole == 1)
                        {
                            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                        }
                        return View(users);
                    }

                    if (!Regex.IsMatch(users.user_password, @"[\W_]"))
                    {
                        ModelState.AddModelError("user_password", "La contraseña debe contener al menos un símbolo.");                       
                        if (actorRole == 1)
                        {
                            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                        }
                        return View(users);
                    }

                    if (actorRole == 2)
                    {
                        users.role_id = 3;
                    }

                    users.user_active = true;
                    users.user_password = PasswordHelper.HashPassword(users.user_password);

                    db.Users.Add(users);
                    db.SaveChanges();

                    Addresses address = new Addresses();

                    address.address_name = Request["address_name"];
                    address.address_description = Request["address_description"];
                    address.address_active = true;
                    address.user_id = users.user_id;

                    decimal latitude;
                    decimal longitude;

                    decimal.TryParse(Request["address_latitude"].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out latitude);
                    decimal.TryParse(Request["address_longitude"].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out longitude);

                    address.address_latitude = latitude;
                    address.address_longitude = longitude;

                    db.Addresses.Add(address);
                    db.SaveChanges();

                    TempData["Success"] = "Usuario creado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.ToString();
                    if (actorRole == 1)
                    {
                        ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                    }
                    return View(users);
                }
            }

            if (actorRole == 1)
            {
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
            }
            return View(users);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId == 3)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Details", new { id = Convert.ToInt32(Session["UserId"]) });
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Users users = db.Users.Find(id);

            if (roleId == 2 && users.role_id == 1)
            {
                TempData["Error"] = "No puede editar administradores.";
                return RedirectToAction("Index");
            }

            if (users == null)
            {
                return HttpNotFound();
            }
            if (roleId == 1)
            {
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
            }
            return View(users);
        }

        // POST: Users/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "user_id,user_name,user_lastname,user_nickname,user_phone,user_email,user_active,role_id")] Users users)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId == 3)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Details", new { id = Convert.ToInt32(Session["UserId"]) });
            }            
            
            if (ModelState.IsValid)
            {
                string email = users.user_email?.Trim();
                bool emailExists = db.Users.Any(u => u.user_email.ToLower() == email.ToLower() && u.user_id != users.user_id);

                if (emailExists)
                {
                    ModelState.AddModelError("user_email", "Este correo ya está en uso.");
                    if (roleId == 1)
                    {
                        ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
                    }
                    return View(users);
                }
                try
                {
                    Users originalUser = db.Users.FirstOrDefault(u => u.user_id == users.user_id);

                    if (roleId == 2 && originalUser.role_id == 1)
                    {
                        TempData["Error"] = "No puede editar administradores.";
                        return RedirectToAction("Index");
                    }

                    if (originalUser == null)
                    {
                        TempData["Error"] = "Usuario no encontrado.";
                        return RedirectToAction("Index");
                    }

                    originalUser.user_name = users.user_name;
                    originalUser.user_lastname = users.user_lastname;
                    originalUser.user_nickname = users.user_nickname;
                    originalUser.user_phone = users.user_phone;
                    originalUser.user_email = users.user_email;
                    originalUser.user_active = users.user_active;

                    if (roleId == 1)
                    {
                        originalUser.role_id = users.role_id;
                    }
                    db.SaveChanges();

                    var address = db.Addresses.FirstOrDefault(a => a.user_id == users.user_id);

                    if (address != null)
                    {
                        address.address_name = Request["address_name"];
                        address.address_description = Request["address_description"];

                        decimal latitude;
                        decimal longitude;

                        decimal.TryParse(Request["address_latitude"].Replace(".", ","), out latitude);
                        decimal.TryParse(Request["address_longitude"].Replace(".", ","), out longitude);

                        address.address_latitude = latitude;
                        address.address_longitude = longitude;

                        db.SaveChanges();
                    }

                    TempData["Success"] = "Usuario actualizado correctamente.";

                    return RedirectToAction("Details", new { id = users.user_id });
                }
                catch
                {
                    TempData["Error"] = "No se pudo actualizar la información.";

                    return RedirectToAction("Details", new { id = users.user_id });
                }
            }

            if (roleId == 1)
            {
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
            }
            return View(users);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Details", new { id = Convert.ToInt32(Session["UserId"]) });
                }

                if (id == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                Users users = db.Users.Find(id);

                if (users == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                if (roleId == 2 && users.role_id == 1)
                {
                    TempData["Error"] = "No puede desactivar administradores.";
                    return RedirectToAction("Index");
                }
                return View(users);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index");
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Details", new { id = Convert.ToInt32(Session["UserId"]) });
                }

                Users users = db.Users.Find(id);

                if (users == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                if (users.user_id == Convert.ToInt32(Session["UserId"]))
                {
                    TempData["Error"] = "No puede desactivar su propio usuario.";
                    return RedirectToAction("Index");
                }

                if (roleId == 2 && users.role_id == 1)
                {
                    TempData["Error"] = "No puede desactivar administradores.";
                    return RedirectToAction("Index");
                }

                users.user_active = false;
                db.SaveChanges();

                TempData["Success"] = "Usuario desactivado correctamente.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "No se pudo desactivar el usuario.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Reactivate(int? id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId != 1)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index");
                }

                if (id == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                Users users = db.Users.Find(id);

                if (users == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                if (users.user_active)
                {
                    TempData["Error"] = "El usuario ya se encuentra activo.";
                    return RedirectToAction("Details", new { id = users.user_id });
                }
                return View(users);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Reactivate")]
        [ValidateAntiForgeryToken]
        public ActionResult ReactivateConfirmed(int id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId != 1)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index");
                }

                Users users = db.Users.Find(id);

                if (users == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                if (users.user_active)
                {
                    TempData["Error"] = "El usuario ya se encuentra activo.";
                    return RedirectToAction("Details", new { id = users.user_id });
                }

                users.user_active = true;
                db.SaveChanges();

                TempData["Success"] = "Usuario reactivado correctamente.";
                return RedirectToAction("Details", new { id = users.user_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo reactivar el usuario.";
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
