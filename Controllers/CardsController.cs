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

                if (roleId == 3 && !card.card_active)
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
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                int roleId = Convert.ToInt32(Session["RoleId"]);
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Index", "Home");
                }

                Cards card = db.Cards.Find(id);

                if (card == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Details", "Users", new { id = card.user_id });
                }

                if (card.user_id == currentUserId)
                {
                    TempData["Error"] = "No puede editar tarjetas asociadas a su propia cuenta.";
                    return RedirectToAction("Details", new { id = card.card_id });
                }

                ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", card.frequency_id);
                ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", card.item_id);
                return View(card);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Users");
            }
        }

        // POST: Cards/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "card_id,card_payday,card_payment_amount,card_item, card_item_price,card_state,card_active,user_id,frequency_id,item_id")] Cards cards)
        {
            int roleId = Convert.ToInt32(Session["RoleId"]);
            int currentUserId = Convert.ToInt32(Session["UserId"]);

            if (roleId == 3)
            {
                TempData["Error"] = "Acceso denegado.";
                return RedirectToAction("Details", new { id = cards.card_id });
            }

            if (ModelState.IsValid)
            {
                bool frequencyExists = db.Frequencies.Any(f => f.frequency_id == cards.frequency_id);

                if (!frequencyExists)
                {
                    ModelState.AddModelError("frequency_id", "La frecuencia seleccionada no existe.");
                    ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
                    ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", cards.item_id);
                    return View(cards);
                }

                if (cards.item_id.HasValue)
                {
                    bool itemExists = db.Items.Any(i => i.item_id == cards.item_id.Value);

                    if (!itemExists)
                    {
                        ModelState.AddModelError("item_id", "El producto seleccionado no existe.");

                        ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
                        ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", cards.item_id);
                        return View(cards);
                    }
                }

                try
                {
                    Cards originalCard = db.Cards.FirstOrDefault(c => c.card_id == cards.card_id);
                    if (originalCard == null)
                    {
                        TempData["Error"] = "Tarjeta no encontrada.";
                        return RedirectToAction("Index", "Users");
                    }

                    if (originalCard.user_id == currentUserId)
                    {
                        TempData["Error"] = "No puede editar tarjetas asociadas a su propia cuenta.";
                        return RedirectToAction("Details", new { id = originalCard.card_id });
                    }

                    originalCard.card_payday = cards.card_payday;
                    originalCard.card_payment_amount = cards.card_payment_amount;
                    originalCard.card_item = cards.card_item;
                    originalCard.card_item_price = cards.card_item_price;
                    originalCard.frequency_id = cards.frequency_id;
                    originalCard.item_id = cards.item_id;

                    db.SaveChanges();
                    TempData["Success"] = "Tarjeta actualizada correctamente.";
                    return RedirectToAction("Details", new { id = originalCard.card_id });
                }
                catch
                {
                    TempData["Error"] = "No se pudo actualizar la tarjeta.";
                    return RedirectToAction("Details", new { id = cards.card_id });
                }
            }

            ViewBag.frequency_id = new SelectList(db.Frequencies, "frequency_id", "frequency_description", cards.frequency_id);
            ViewBag.item_id = new SelectList(db.Items, "item_id", "item_name", cards.item_id);
            return View(cards);
        }

        // GET: Cards/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Details", "Users", new { id = currentUserId });
                }

                if (id == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                Cards cards = db.Cards.Find(id);

                if (cards == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                if (cards.user_id == currentUserId)
                {
                    TempData["Error"] = "No puede desactivar tarjetas asociadas a su propia cuenta.";
                    return RedirectToAction("Details", new { id = cards.card_id });
                }

                return View(cards);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Users");
            }
        }

        // POST: Cards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                if (roleId == 3)
                {
                    TempData["Error"] = "Acceso denegado.";
                    return RedirectToAction("Details", "Users", new { id = currentUserId });
                }

                Cards cards = db.Cards.Find(id);

                if (cards == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                if (cards.user_id == currentUserId)
                {
                    TempData["Error"] = "No puede desactivar tarjetas asociadas a su propia cuenta.";
                    return RedirectToAction("Details", new { id = cards.card_id });
                }

                cards.card_active = false;
                db.SaveChanges();

                TempData["Success"] = "Tarjeta desactivada correctamente.";

                return RedirectToAction("Details", "Users", new { id = cards.user_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo desactivar la tarjeta.";

                Cards card = db.Cards.Find(id);

                if (card != null)
                {
                    return RedirectToAction("Details", "Users", new { id = card.user_id });
                }

                return RedirectToAction("Index", "Users");
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
                    return RedirectToAction("Index", "Users");
                }

                if (id == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                Cards cards = db.Cards.Find(id);

                if (cards == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                if (cards.card_active)
                {
                    TempData["Error"] = "La tarjeta ya se encuentra activa.";
                    return RedirectToAction("Details", new { id = cards.card_id });
                }

                return View(cards);
            }
            catch
            {
                TempData["Error"] = "No se pudo obtener la información.";
                return RedirectToAction("Index", "Users");
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
                    return RedirectToAction("Index", "Users");
                }

                Cards cards = db.Cards.Find(id);

                if (cards == null)
                {
                    TempData["Error"] = "Tarjeta no encontrada.";
                    return RedirectToAction("Index", "Users");
                }

                if (cards.card_active)
                {
                    TempData["Error"] = "La tarjeta ya se encuentra activa.";
                    return RedirectToAction("Details", new { id = cards.card_id });
                }

                cards.card_active = true;
                db.SaveChanges();

                TempData["Success"] = "Tarjeta reactivada correctamente.";

                return RedirectToAction("Details", "Users", new { id = cards.user_id });
            }
            catch
            {
                TempData["Error"] = "No se pudo reactivar la tarjeta.";
                return RedirectToAction("Index", "Users");
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
