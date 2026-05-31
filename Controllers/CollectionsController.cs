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
    public class CollectionsController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        // GET: Collections
        public ActionResult Index()
        {
            var collections = db.Collections.Include(c => c.Cards).Include(c => c.Users);
            return View(collections.ToList());
        }

        // GET: Collections/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collections collections = db.Collections.Find(id);
            if (collections == null)
            {
                return HttpNotFound();
            }
            return View(collections);
        }

        // GET: Collections/Create
        public ActionResult Create()
        {
            ViewBag.card_id = new SelectList(db.Cards, "card_id", "card_payday");
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name");
            return View();
        }

        // POST: Collections/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "collection_id,collection_date,collection_amount,collection_balance,collection_note,collection_active,user_id,card_id")] Collections collections)
        {
            if (ModelState.IsValid)
            {
                db.Collections.Add(collections);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.card_id = new SelectList(db.Cards, "card_id", "card_payday", collections.card_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name", collections.user_id);
            return View(collections);
        }

        // GET: Collections/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collections collections = db.Collections.Find(id);
            if (collections == null)
            {
                return HttpNotFound();
            }
            ViewBag.card_id = new SelectList(db.Cards, "card_id", "card_payday", collections.card_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name", collections.user_id);
            return View(collections);
        }

        // POST: Collections/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "collection_id,collection_date,collection_amount,collection_balance,collection_note,collection_active,user_id,card_id")] Collections collections)
        {
            if (ModelState.IsValid)
            {
                db.Entry(collections).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.card_id = new SelectList(db.Cards, "card_id", "card_payday", collections.card_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name", collections.user_id);
            return View(collections);
        }

        // GET: Collections/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Collections collections = db.Collections.Find(id);
            if (collections == null)
            {
                return HttpNotFound();
            }
            return View(collections);
        }

        // POST: Collections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Collections collections = db.Collections.Find(id);
            db.Collections.Remove(collections);
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
