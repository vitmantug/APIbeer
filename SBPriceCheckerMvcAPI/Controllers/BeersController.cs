using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SBPriceCheckerMvcAPI.DAL;
using SBPriceCheckerMvcAPI.Models;

namespace SBPriceCheckerMvcAPI.Controllers
{
    public class BeersController : Controller
    {
        private APIBeerContext db = new APIBeerContext();

        // GET: Beers
        public async Task<ActionResult> Index()
        {
            return View(await db.Beers.ToListAsync());
        }

        // GET: Beers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Beer beer = await db.Beers.FindAsync(id);
            if (beer == null)
            {
                return HttpNotFound();
            }
            return View(beer);
        }

        // GET: Beers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Beers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BeerID,id,name,total,capacity,priceBefore,priceAfter,pricePerLitre,priceUnity,hasDiscount,discountType,discountValue,discountNote,promoStart,promoEnd,store,imageUrl,detailsUrl")] Beer beer)
        {
            if (ModelState.IsValid)
            {
                db.Beers.Add(beer);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(beer);
        }

        // GET: Beers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Beer beer = await db.Beers.FindAsync(id);
            if (beer == null)
            {
                return HttpNotFound();
            }
            return View(beer);
        }

        // POST: Beers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BeerID,id,name,total,capacity,priceBefore,priceAfter,pricePerLitre,priceUnity,hasDiscount,discountType,discountValue,discountNote,promoStart,promoEnd,store,imageUrl,detailsUrl")] Beer beer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(beer).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(beer);
        }

        // GET: Beers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Beer beer = await db.Beers.FindAsync(id);
            if (beer == null)
            {
                return HttpNotFound();
            }
            return View(beer);
        }

        // POST: Beers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Beer beer = await db.Beers.FindAsync(id);
            db.Beers.Remove(beer);
            await db.SaveChangesAsync();
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
