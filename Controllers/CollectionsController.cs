using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SistemaGestionVentas.Filters;
using SistemaGestionVentas.Models;
using PagedList;
using SistemaGestionVentas.Models.ViewModels;

namespace SistemaGestionVentas.Controllers
{
    [SessionAuthorize]
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
        public ActionResult Create(int cardId)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId == 3)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index", "Home");
            }

            Cards card = db.Cards.FirstOrDefault(c => c.card_id == cardId);

            if (card == null)
            {
                TempData["Error"] = "Tarjeta no encontrada.";
                return RedirectToAction("Index", "Users");
            }

            if (!card.card_state)
            {
                TempData["Error"] = "La tarjeta ya se encuentra saldada.";
                return RedirectToAction("Details", "Cards", new { id = cardId });
            }

            int currentUserId = Convert.ToInt32(Session["UserId"]);

            if (currentUserId == card.user_id)
            {
                TempData["Error"] = "No puede registrar cobros en su propia cuenta.";
                return RedirectToAction("Details", "Cards", new { id = cardId });
            }

            Collections collection = new Collections();
            collection.card_id = cardId;
            return View(collection);
        }

        // POST: Collections/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "collection_id,collection_date,collection_amount,collection_balance,collection_note,collection_active,user_id,card_id")] Collections collections)
        {
            if (collections.collection_amount == 0 && string.IsNullOrWhiteSpace(collections.collection_note))
            {
                ModelState.AddModelError("collection_note", "Debe ingresar una nota cuando el monto es cero.");
            }

            if (!ModelState.IsValid)
            {
                return View(collections);
            }

            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                Cards card = db.Cards.FirstOrDefault(c => c.card_id == collections.card_id);

                if (card == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                if (!card.card_state)
                {
                    TempData["Error"] = "La tarjeta ya se encuentra saldada.";
                    return RedirectToAction("Details", "Cards", new { id = card.card_id });
                }

                int currentUserId = Convert.ToInt32(Session["UserId"]);

                if (currentUserId == card.user_id)
                {
                    TempData["Error"] = "No puede registrar cobros en su propia cuenta.";
                    return RedirectToAction("Details", "Cards", new { id = card.card_id });
                }

                int saldoActual;
                Collections ultimoCobro = db.Collections.Where(c => c.card_id == card.card_id && c.collection_active).OrderByDescending(c => c.collection_id).FirstOrDefault();

                if (ultimoCobro == null)
                {
                    saldoActual = card.card_item_price;
                }
                else
                {
                    saldoActual = ultimoCobro.collection_balance;
                }

                if (collections.collection_amount > saldoActual)
                {
                    ModelState.AddModelError("collection_amount", "El monto no puede ser mayor al saldo pendiente.");
                    return View(collections);
                }

                int nuevoSaldo = saldoActual - collections.collection_amount;

                collections.collection_date = DateTime.Now;
                collections.collection_balance = nuevoSaldo;
                collections.collection_active = true;
                collections.user_id = currentUserId;

                db.Collections.Add(collections);

                if (nuevoSaldo == 0)
                {
                    card.card_state = false;
                }

                db.SaveChanges();
                TempData["Success"] = "Cobro registrado correctamente.";
                return RedirectToAction("Details", "Cards", new { id = card.card_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo registrar el cobro.";
                return View(collections);
            }
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
