using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using UrlShortener2._0.Models;

namespace UrlShortener2._0.Controllers
{
    public class BookMarkController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private static string ShortUrl(string Url)
        {

            byte[] byteData = Encoding.UTF8.GetBytes(Url);
            Stream inputStream = new MemoryStream(byteData);
            using (SHA256 shall = new SHA256Managed())
            {
                var result = shall.ComputeHash(inputStream);
                string output = BitConverter.ToString(result);
                string Hashed = (output.Replace("-", "").Substring(0, 5));
                return Hashed;
            }

        }
        // GET: api/BookMark
        public IQueryable<BookMark> GetBookmarks()
        {
            var currentUser = User.Identity.GetUserId();
            return db.Bookmarks.Where(o => (o.Public == true) || (o.OwnerId == currentUser));
        }

        // GET: api/BookMark/5
        [ResponseType(typeof(BookMark))]
        public async Task<IHttpActionResult> GetBookMark(int id)
        {
            BookMark bookMark = await db.Bookmarks.FindAsync(id);
            if (bookMark == null)
            {
                return NotFound();
            }
            var currentUser = User.Identity.GetUserId();
            return Ok(db.Bookmarks.Where(o => (o.Public == true) || (o.OwnerId == currentUser)));

            
        }

        // PUT: api/BookMark/5
        [System.Web.Http.Authorize]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBookMark(int id, BookMark bookMark)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bookMark.Id)
            {
                return BadRequest();
            }

            db.Entry(bookMark).State = EntityState.Modified;

            try
            {
                if (bookMark.OwnerId == User.Identity.GetUserId())
                {
                    bookMark.ShortUrl = ShortUrl(bookMark.Url);
                    bookMark.Created = DateTime.Now;
                    bookMark.Click = bookMark.Click;
                    bookMark.OwnerId = User.Identity.GetUserId();
                    await db.SaveChangesAsync();
                }
               else
                {
                    return BadRequest();
                }
                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookMarkExists(id))
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

        // POST: api/BookMark
        [ResponseType(typeof(BookMark))]
        public async Task<IHttpActionResult> PostBookMark(BookMark bookMark)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Bookmarks.Add(bookMark);
            bookMark.ShortUrl = ShortUrl(bookMark.Url);
            bookMark.Created = DateTime.Now;
            bookMark.Click = 0;
            bookMark.OwnerId = User.Identity.GetUserId();
            if (bookMark.OwnerId == null)
            { bookMark.Public = true; }
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = bookMark.Id }, bookMark);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookMarkExists(int id)
        {
            return db.Bookmarks.Count(e => e.Id == id) > 0;
        }
    }
}