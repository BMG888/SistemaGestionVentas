using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    public class UsersController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        // GET: Users
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Roles);
            return View(users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
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
        public ActionResult Create([Bind(Include = "user_id,user_name,user_lastname,user_nickname,user_phone,user_email,user_password,user_active,role_id")] Users users)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(users);
                db.SaveChanges();

                Addresses address = new Addresses();

                address.address_name = Request["address_name"];
                address.address_description = Request["address_description"];
                address.address_active = true;
                address.user_id = users.user_id;

                decimal latitude;
                decimal longitude;

                decimal.TryParse(Request["address_latitude"], out latitude);
                decimal.TryParse(Request["address_longitude"], out longitude);

                address.address_latitude = latitude;
                address.address_longitude = longitude;

                db.Addresses.Add(address);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
            return View(users);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
            return View(users);
        }

        // POST: Users/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "user_id,user_name,user_lastname,user_nickname,user_phone,user_email,user_password,user_active,role_id")] Users users)
        {
            if (ModelState.IsValid)
            {
                db.Entry(users).State = EntityState.Modified;
                db.SaveChanges();

                var address = db.Addresses.FirstOrDefault(a => a.user_id == users.user_id);

                if (address != null)
                {
                    address.address_name = Request["address_name"];
                    address.address_description = Request["address_description"];
                    address.address_active = true;
                    address.user_id = users.user_id;

                    decimal latitude;
                    decimal longitude;

                    decimal.TryParse(Request["address_latitude"], out latitude);
                    decimal.TryParse(Request["address_longitude"], out longitude);

                    address.address_latitude = latitude;
                    address.address_longitude = longitude;

                    db.Entry(address).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_description", users.role_id);
            return View(users);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Users users = db.Users.Find(id);
            db.Users.Remove(users);
            db.SaveChanges();
            return RedirectToAction("Index");
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
