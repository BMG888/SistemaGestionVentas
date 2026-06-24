using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SistemaGestionVentas.Models;
using PagedList;
using SistemaGestionVentas.Filters;
using SistemaGestionVentas.Models.ViewModels;

namespace SistemaGestionVentas.Controllers
{
    public class AlbumsController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        // GET: Albums
        public ActionResult Index(string album_name, bool? album_active, int? page)
        {
            int? roleId = Session["RoleId"] != null ? Convert.ToInt32(Session["RoleId"]) : (int?)null;
            var albums = db.Albums.AsQueryable();
            
            if (roleId == null || roleId == 3)
            {
                albums = albums.Where(a => a.album_active);
            }
          
            if (!string.IsNullOrWhiteSpace(album_name))
            {
                albums = albums.Where(a => a.album_name.Contains(album_name));
            }
            
            if ((roleId == 1 || roleId == 2) && album_active.HasValue)
            {
                albums = albums.Where(a => a.album_active == album_active.Value);
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(albums.OrderBy(a => a.album_name).ToPagedList(pageNumber, pageSize));
        }

        // GET: Albums/Details/5
        public ActionResult Details(int? id, string item_name, bool? item_active, int page = 1)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                int roleId = Session["RoleId"] != null ? Convert.ToInt32(Session["RoleId"]) : 0;
                Albums album = db.Albums.Find(id);

                if (album == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }
                
                if ((roleId == 0 || roleId == 3) && !album.album_active)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                var itemsQuery = db.Items.Where(i => i.album_id == album.album_id);

                if (!string.IsNullOrWhiteSpace(item_name))
                {
                    itemsQuery = itemsQuery.Where(i => i.item_name.Contains(item_name));
                }
                
                if (roleId == 0 || roleId == 3)
                {
                    itemsQuery = itemsQuery.Where(i => i.item_active);
                }
                else
                {
                    if (item_active.HasValue)
                    {
                        itemsQuery = itemsQuery.Where(i => i.item_active == item_active.Value);
                    }
                }

                int pageSize = 5;
                var viewModel = new AlbumDetailsViewModel
                {
                    Album = album,
                    Items = itemsQuery.OrderBy(i => i.item_name).ToPagedList(page, pageSize),
                    ItemNameFilter = item_name,
                    ItemActiveFilter = item_active
                };
                return View(viewModel);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index");
            }
        }

        // GET: Albums/Create
        [SessionAuthorize]
        public ActionResult Create()
        {
            if (Session["RoleId"] == null || (Convert.ToInt32(Session["RoleId"]) != 1 && Convert.ToInt32(Session["RoleId"]) != 2))
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        
        // POST: Albums/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult Create([Bind(Include = "album_id,album_name,album_active")] Albums albums)
        {
            if (!ModelState.IsValid)
            {
                return View(albums);
            }

            try
            {
                if (Session["RoleId"] == null || (Convert.ToInt32(Session["RoleId"]) != 1 &&  Convert.ToInt32(Session["RoleId"]) != 2))
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                albums.album_name = albums.album_name.Trim();
                bool albumExist = db.Albums.Any(a => a.album_name.ToLower() == albums.album_name.ToLower());

                if (albumExist)
                {
                    ModelState.AddModelError("album_name", "Ya existe un álbum con ese nombre.");
                    return View(albums);
                }

                albums.album_active = true;
                db.Albums.Add(albums);
                db.SaveChanges();
                TempData["Success"] = "Álbum creado correctamente.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "No se pudo crear el álbum.";
                return View(albums);
            }
        }

        // GET: Albums/Edit/5
        [SessionAuthorize]
        public ActionResult Edit(int? id)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId != 1 && roleId != 2)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Albums album = db.Albums.Find(id);

            if (album == null)
            {
                TempData["Error"] = "Álbum no encontrado.";
                return RedirectToAction("Index");
            }

            return View(album);
        }

        // POST: Albums/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public ActionResult Edit([Bind(Include = "album_id,album_name")] Albums albums)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId != 1 && roleId != 2)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                albums.album_name = albums.album_name.Trim();
                bool albumExist = db.Albums.Any(a => a.album_name == albums.album_name && a.album_id != albums.album_id);

                if (albumExist)
                {
                    ModelState.AddModelError("album_name", "Ya existe un álbum registrado con ese nombre.");
                    return View(albums);
                }

                try
                {
                    Albums originalAlbum = db.Albums.FirstOrDefault(a => a.album_id == albums.album_id);

                    if (originalAlbum == null)
                    {
                        TempData["Error"] = "Álbum no encontrado.";
                        return RedirectToAction("Index");
                    }

                    originalAlbum.album_name = albums.album_name;
                    db.SaveChanges();
                    TempData["Success"] = "Álbum actualizado correctamente.";
                    return RedirectToAction("Details", new { id = albums.album_id });
                }
                catch
                {
                    TempData["Error"] = "No se pudo actualizar el álbum.";
                    return RedirectToAction("Details", new { id = albums.album_id });
                }
            }
            return View(albums);
        }

        // GET: Albums/Delete/5
        [SessionAuthorize]
        public ActionResult Delete(int? id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId != 1 && roleId != 2)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index");
                }

                if (id == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                Albums albums = db.Albums.Find(id);

                if (albums == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                if (!albums.album_active)
                {
                    TempData["Error"] = "El álbum ya se encuentra desactivado.";
                    return RedirectToAction("Details", new { id = albums.album_id });
                }

                return View(albums);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index");
            }
        }

        // POST: Albums/Delete/5
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
                    return RedirectToAction("Index");
                }

                Albums albums = db.Albums.Find(id);

                if (albums == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                if (!albums.album_active)
                {
                    TempData["Error"] = "El álbum ya se encuentra desactivado.";
                    return RedirectToAction("Details", new { id = albums.album_id });
                }

                albums.album_active = false;
                db.SaveChanges();
                TempData["Success"] = "Álbum desactivado correctamente.";
                return RedirectToAction("Details", new { id = albums.album_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo desactivar el álbum.";
                return RedirectToAction("Index");
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
                    return RedirectToAction("Index");
                }

                if (id == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                Albums albums = db.Albums.Find(id);

                if (albums == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                if (albums.album_active)
                {
                    TempData["Error"] = "El álbum ya se encuentra activo.";
                    return RedirectToAction("Details", new { id = albums.album_id });
                }

                return View(albums);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index");
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
                    return RedirectToAction("Index");
                }

                Albums albums = db.Albums.Find(id);

                if (albums == null)
                {
                    TempData["Error"] = "Álbum no encontrado.";
                    return RedirectToAction("Index");
                }

                if (albums.album_active)
                {
                    TempData["Error"] = "El álbum ya se encuentra activo.";
                    return RedirectToAction("Details", new { id = albums.album_id });
                }

                albums.album_active = true;
                db.SaveChanges();
                TempData["Success"] = "Álbum reactivado correctamente.";
                return RedirectToAction("Details", new { id = albums.album_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo reactivar el álbum.";
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
