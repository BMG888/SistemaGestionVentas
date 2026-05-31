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
    public class CardsController : Controller
    {
        private SistemaGestionVentasDBEntities db = new SistemaGestionVentasDBEntities();

        // GET: Cards
        public ActionResult Index()
        {
            var cards = db.Cards.Include(c => c.Frequencies).Include(c => c.Items).Include(c => c.Users);
            return View(cards.ToList());
        }

        // GET: Cards/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cards cards = db.Cards.Find(id);
            if (cards == null)
            {
                return HttpNotFound();
            }
            return View(cards);
        }

        // GET: Cards/Create
        public ActionResult Create()
        {
            ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description");
            ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name");
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name");
            return View();
        }

        // POST: Cards/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "card_id,card_payday,card_payment_amount,card_item,card_state,card_active,user_id,frequency_id,item_id")] Cards cards)
        {
            if (ModelState.IsValid)
            {
                db.Cards.Add(cards);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
            ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", cards.item_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name", cards.user_id);
            return View(cards);
        }

        // GET: Cards/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cards cards = db.Cards.Find(id);
            if (cards == null)
            {
                return HttpNotFound();
            }
            ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
            ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", cards.item_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name", cards.user_id);
            return View(cards);
        }

        // POST: Cards/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "card_id,card_payday,card_payment_amount,card_item,card_state,card_active,user_id,frequency_id,item_id")] Cards cards)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cards).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
            ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", cards.item_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "user_name", cards.user_id);
            return View(cards);
        }

        // GET: Cards/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cards cards = db.Cards.Find(id);
            if (cards == null)
            {
                return HttpNotFound();
            }
            return View(cards);
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cards cards = db.Cards.Find(id);
            db.Cards.Remove(cards);
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
