using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SBPriceCheckerMvcAPI.DAL;
using SBPriceCheckerMvcAPI.Models;

namespace SBPriceCheckerMvcAPI.Controllers
{
    public class ApiBeersController : ApiController
    {
        private APIBeerContext db = new APIBeerContext();

        // GET: api/ApiBeers
        public IQueryable<Beer> GetBeers()
        {
            return db.Beers;
        }

        // GET: api/ApiBeers/5
        [ResponseType(typeof(Beer))]
        public async Task<IHttpActionResult> GetBeer(int id)
        {
            Beer beer = await db.Beers.FindAsync(id);
            if (beer == null)
            {
                return NotFound();
            }

            return Ok(beer);
        }

        // PUT: api/ApiBeers/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBeer(int id, Beer beer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != beer.BeerID)
            {
                return BadRequest();
            }

            db.Entry(beer).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BeerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ApiBeers
        [ResponseType(typeof(Beer))]
        public async Task<IHttpActionResult> PostBeer(Beer beer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Beers.Add(beer);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = beer.BeerID }, beer);
        }

        // DELETE: api/ApiBeers/5
        [ResponseType(typeof(Beer))]
        public async Task<IHttpActionResult> DeleteBeer(int id)
        {
            Beer beer = await db.Beers.FindAsync(id);
            if (beer == null)
            {
                return NotFound();
            }

            db.Beers.Remove(beer);
            await db.SaveChangesAsync();

            return Ok(beer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BeerExists(int id)
        {
            return db.Beers.Count(e => e.BeerID == id) > 0;
        }
    }
}