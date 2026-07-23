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

                string validationError;
                if (!IsValidImageFile(imageFile, out validationError))
                {
                    ModelState.AddModelError("item_url", validationError);
                    return View(items);
                }

                items.item_name = items.item_name?.Trim();
                items.item_description = items.item_description?.Trim();
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
            if (Session["RoleId"] == null || (Convert.ToInt32(Session["RoleId"]) != 1 && Convert.ToInt32(Session["RoleId"]) != 2))
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Items item = db.Items.FirstOrDefault(i => i.item_id == id);

            if (item == null)
            {
                TempData["Error"] = "Ítem no encontrado.";
                return RedirectToAction("Index", "Albums");
            }
            ViewBag.album_id = new SelectList(db.Albums, "album_id", "album_name", item.album_id);
            return View(item);
        }

        // POST: Items/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult Edit([Bind(Include = "item_id,item_name,item_url,item_description,album_id")] Items items, HttpPostedFileBase imageFile)
        {
            if (Session["RoleId"] == null || (Convert.ToInt32(Session["RoleId"]) != 1 && Convert.ToInt32(Session["RoleId"]) != 2))
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.album_id = new SelectList(db.Albums, "album_id", "album_name", items.album_id);
                return View(items);
            }

            try
            {
                Albums album = db.Albums.FirstOrDefault(a => a.album_id == items.album_id);
                if (album == null)
                {
                    TempData["Error"] = "Álbum no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                Items originalItem = db.Items.FirstOrDefault(i => i.item_id == items.item_id);
                if (originalItem == null)
                {
                    TempData["Error"] = "Ítem no encontrado.";
                    return RedirectToAction("Index", "Albums");
                }

                bool fileWasSent = imageFile != null && imageFile.ContentLength > 0;

                if (fileWasSent)
                {
                    string validationError;
                    if (!IsValidImageFile(imageFile, out validationError))
                    {
                        ModelState.AddModelError("item_url", validationError);
                        ViewBag.album_id = new SelectList(db.Albums, "album_id", "album_name", items.album_id);
                        return View(items);
                    }
                }

                items.item_name = items.item_name?.Trim();
                items.item_description = items.item_description?.Trim();
                originalItem.item_name = items.item_name;
                originalItem.item_description = items.item_description;
                originalItem.album_id = items.album_id;

                if (fileWasSent)
                {
                    if (!string.IsNullOrWhiteSpace(originalItem.item_url))
                    {
                        string oldPath = Server.MapPath(originalItem.item_url);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    string extension = Path.GetExtension(imageFile.FileName);
                    string fileName = Guid.NewGuid().ToString() + extension;
                    string path = Path.Combine(Server.MapPath("~/Images/Items"), fileName);
                    imageFile.SaveAs(path);
                    originalItem.item_url = "/Images/Items/" + fileName;
                }

                db.SaveChanges();
                TempData["Success"] = "Ítem actualizado correctamente.";
                return RedirectToAction("Details", new { id = items.item_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo actualizar el ítem.";
                ViewBag.album_id = new SelectList(db.Albums, "album_id", "album_name", items.album_id);
                return View(items);
            }
        }

        // GET: Items/Delete/5
        [SessionAuthorize]
        public ActionResult Delete(int? id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1 && roleId != 2)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                if (id == null)
                {
                    TempData["Error"] = "Ítem no encontrado o no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                Items item = db.Items.Find(id);
                if (item == null)
                {
                    TempData["Error"] = "Ítem no encontrado o no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                if (!item.item_active)
                {
                    TempData["Error"] = "El ítem ya se encuentra desactivado.";
                    return RedirectToAction("Details", new { id = item.item_id });
                }

                return View(item);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Albums");
            }
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1 && roleId != 2)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                Items item = db.Items.Find(id);
                if (item == null)
                {
                    TempData["Error"] = "Ítem no encontrado o no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                if (!item.item_active)
                {
                    TempData["Error"] = "El ítem ya se encuentra desactivado.";
                    return RedirectToAction("Details", new { id = item.item_id });
                }

                item.item_active = false;
                db.SaveChanges();
                TempData["Success"] = "Ítem desactivado correctamente.";
                return RedirectToAction("Details", new { id = item.item_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo desactivar el ítem.";
                return RedirectToAction("Index", "Albums");
            }
        }

        [SessionAuthorize]
        public ActionResult Reactivate(int? id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                if (id == null)
                {
                    TempData["Error"] = "Ítem no encontrado o no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                Items item = db.Items.Find(id);
                if (item == null)
                {
                    TempData["Error"] = "Ítem no encontrado o no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                if (item.item_active)
                {
                    TempData["Error"] = "El ítem ya se encuentra activo.";
                    return RedirectToAction("Details", new { id = item.item_id });
                }

                return View(item);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Albums");
            }
        }

        [HttpPost, ActionName("Reactivate")]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult ReactivateConfirmed(int id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                Items item = db.Items.Find(id);
                if (item == null)
                {
                    TempData["Error"] = "Ítem no encontrado o no disponible.";
                    return RedirectToAction("Index", "Albums");
                }

                if (item.item_active)
                {
                    TempData["Error"] = "El ítem ya se encuentra activo.";
                    return RedirectToAction("Details", new { id = item.item_id });
                }

                item.item_active = true;
                db.SaveChanges();
                TempData["Success"] = "Ítem reactivado correctamente.";
                return RedirectToAction("Details", new { id = item.item_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo reactivar el ítem.";
                return RedirectToAction("Index", "Albums");
            }
        }

        [HttpGet]
        [SessionAuthorize]
        public JsonResult GetAlbums()
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1 && roleId != 2)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var albums = db.Albums.OrderBy(a => a.album_name).Select(a => new { id = a.album_id, name = a.album_name }).ToList();
                return Json(new { success = true, albums = albums }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [SessionAuthorize]
        public JsonResult GetItems(int albumId)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1 && roleId != 2)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                bool albumExists = db.Albums.Any(a => a.album_id == albumId);
                if (!albumExists)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var items = db.Items.Where(i => i.album_id == albumId).OrderBy(i => i.item_name).Select(i => new { id = i.item_id, name = i.item_name, image = i.item_url, albumId = i.album_id }).ToList();
                return Json(new { success = true, items = items }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [SessionAuthorize]
        public JsonResult GetItem(int itemId)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                if (roleId != 1 && roleId != 2)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var item = db.Items.Where(i => i.item_id == itemId).Select(i => new { id = i.item_id, name = i.item_name, image = i.item_url, albumId = i.album_id }).FirstOrDefault();
                if (item == null)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true, item }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private bool IsValidImageFile(HttpPostedFileBase file, out string errorMessage)
        {
            errorMessage = null;

            if (file == null || file.ContentLength == 0)
            {
                errorMessage = "Debe seleccionar una imagen.";
                return false;
            }

            if (file.ContentLength > MaxFileSizeBytes)
            {
                errorMessage = "La imagen no puede superar los 5 MB.";
                return false;
            }

            string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                errorMessage = "Formato no permitido. Solo se aceptan JPG, JPEG, PNG y WEBP.";
                return false;
            }

            if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = "El archivo no parece ser una imagen válida.";
                return false;
            }

            return true;
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
