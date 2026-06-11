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
using PagedList;
using SistemaGestionVentas.Models.ViewModels;

namespace SistemaGestionVentas.Controllers
{
    [SessionAuthorize]
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
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                int roleId = Convert.ToInt32(Session["RoleId"]);
                int userId = Convert.ToInt32(Session["UserId"]);

                Cards card = db.Cards.Include(c => c.Users).Include(c => c.Frequencies).FirstOrDefault(c => c.card_id == id);

                if (card == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                if(roleId == 3 && !card.card_active)
{
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Details", "Users", new { id = userId });
                }

                if (roleId == 3 && card.user_id != userId)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Details", "Users", new { id = userId });
                }

                var viewModel = new CardDetailsViewModel
                {
                    Card = card,
                    Collections = Enumerable.Empty<Collections>().ToPagedList(1, 1)
                };
                return View(viewModel);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Users");
            }
        }

        // GET: Cards/Create
        public ActionResult Create(int userId)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);

            if (roleId == 3)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Index", "Home");
            }

            Users user = db.Users.FirstOrDefault(u => u.user_id == userId);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index", "Users");
            }

            int currentUserId = Convert.ToInt32(Session["UserId"]);

            if (currentUserId == userId)
            {
                TempData["Error"] = "No puede registrar tarjetas en su propia cuenta.";
                return RedirectToAction("Details", "Users", new { id = userId });
            }

            ViewBag.UserId = userId;
            ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description");            
            return View();
        }

        // POST: Cards/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "card_id,card_payday,card_payment_amount,card_item, card_item_price,frequency_id,item_id")] Cards cards, int userId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = userId;
                ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
                return View(cards);
            }
            
            if (!db.Frequencies.Any(f => f.frequency_id == cards.frequency_id))
            {
                ModelState.AddModelError("frequency_id", "La frecuencia seleccionada no existe.");
                ViewBag.UserId = userId;
                ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
                return View(cards);
            }

            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                Users user = db.Users.FirstOrDefault(u => u.user_id == userId);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index", "Users");
                }

                int currentUserId = Convert.ToInt32(Session["UserId"]);
                if (currentUserId == userId)
                {
                    TempData["Error"] = "No puede registrar tarjetas en su propia cuenta.";
                    return RedirectToAction("Details", "Users", new { id = userId });
                }

                cards.user_id = userId;
                cards.card_active = true;
                cards.card_state = true;

                db.Cards.Add(cards);
                db.SaveChanges();

                TempData["Success"] = "Tarjeta registrada correctamente.";
                return RedirectToAction("Details", "Users", new { id = userId });
            }
            catch
            {
                TempData["Error"] = "No se pudo registrar la tarjeta.";
                ViewBag.UserId = userId;
                ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
                return View(cards);
            }
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
        public ActionResult Edit([Bind(Include = "card_id,card_payday,card_payment_amount,card_item, card_item_price,card_state,card_active,user_id,frequency_id,item_id")] Cards cards)
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
