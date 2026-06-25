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
using System.IO;

namespace SistemaGestionVentas.Controllers
{
    public class ItemsController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        // GET: Items
        public ActionResult Index()
        {
            var items = db.Items.Include(i => i.Albums);
            return View(items.ToList());
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                int roleId = Session["RoleId"] != null ? Convert.ToInt32(Session["RoleId"]) : 0;
                var item = db.Items.Include("Albums").FirstOrDefault(i => i.item_id == id);

                if (item == null)
                {
                    TempData["Error"] = "Ítem no encontrado.";
                    return RedirectToAction("Index", "Albums");
                }

                if ((roleId == 0 || roleId == 3) && (!item.item_active || !item.Albums.album_active))
                {
                    TempData["Error"] = "Ítem no encontrado.";
                    return RedirectToAction("Index", "Albums");
                }

                return View(item);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Albums");
            }
        }

        // GET: Items/Create
        [SessionAuthorize]
        public ActionResult Create(int albumId)
        {
            if (Session["RoleId"] == null || (Convert.ToInt32(Session["RoleId"]) != 1 && Convert.ToInt32(Session["RoleId"]) != 2))
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index", "Home");
            }

            Albums album = db.Albums.FirstOrDefault(a => a.album_id == albumId);

            if (album == null)
            {
                TempData["Error"] = "Álbum no encontrado.";
                return RedirectToAction("Index", "Albums");
            }

            Items item = new Items
            {
                album_id = albumId
            };

            return View(item);
        }

        // POST: Items/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult Create([Bind(Include = "item_id,item_name,item_url,item_description,item_active,album_id")] Items items, HttpPostedFileBase imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(items);
            }

            try
            {
                if (Session["RoleId"] == null || (Convert.ToInt32(Session["RoleId"]) != 1 && Convert.ToInt32(Session["RoleId"]) != 2))
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                Albums album = db.Albums.FirstOrDefault(a => a.album_id == items.album_id);

                if (album == null)
                {
                    TempData["Error"] = "Álbum no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                if (imageFile == null || imageFile.ContentLength == 0)
                {
                    ModelState.AddModelError("item_url", "Debe seleccionar una imagen.");
                    return View(items);
                }

                items.item_name = items.item_name.Trim();
                items.item_description = items.item_description.Trim();                
                items.item_active = true;

                string extension = Path.GetExtension(imageFile.FileName);
                string fileName = Guid.NewGuid().ToString() + extension;
                string path = Path.Combine(Server.MapPath("~/Images/Items"), fileName);
                imageFile.SaveAs(path);
                items.item_url = "/Images/Items/" + fileName;                

                db.Items.Add(items);
                db.SaveChanges();
                TempData["Success"] = "Ítem registrado correctamente.";
                return RedirectToAction("Details", "Albums", new { id = items.album_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo registrar el ítem.";
                return View(items);
            }
        }

        // GET: Items/Edit/5
        [SessionAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Items items = db.Items.Find(id);
            if (items == null)
            {
                return HttpNotFound();
            }
            ViewBag.album_id = new SelectList(db.Albums, "album_id", "album_name", items.album_id);
            return View(items);
        }

        // POST: Items/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult Edit([Bind(Include = "item_id,item_name,item_url,item_description,item_active,album_id")] Items items)
        {
            if (ModelState.IsValid)
            {
                db.Entry(items).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.album_id = new SelectList(db.Albums, "album_id", "album_name", items.album_id);
            return View(items);
        }

        // GET: Items/Delete/5
        [SessionAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Items items = db.Items.Find(id);
            if (items == null)
            {
                return HttpNotFound();
            }
            return View(items);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Items items = db.Items.Find(id);
            db.Items.Remove(items);
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
