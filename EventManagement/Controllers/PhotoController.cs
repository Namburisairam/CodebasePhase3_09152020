using System.Web.Mvc;
using EventManagement.DataAccess.DataBase.Model;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    public class PhotoController : BaseController
    {
        Entities db = new Entities();
        // GET: Photo
        public ActionResult Create(int id)
        {
            var photo = db.Photos.Find(id);
            return View(photo);
        }
        /// <summary>
        /// Move photos to albums
        /// </summary>
        /// <param name="photoID"></param>
        /// <param name="AlbumID"></param>
        /// <param name="Description"></param>
        /// <param name="IsEventManager"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditImageDetails(int photoID, int? AlbumID, string Description, bool IsEventManager = false)
        {
            var photo = db.Photos.Find(photoID);
            photo.AlbumID = AlbumID;
            photo.Description = Description;
            db.SaveChanges();
            if (!IsEventManager)
            {
                return Redirect(Url.Action("Create", "Events", new { Id = photo.EventID }) + "#Photos");
            }
            else
            {
                return Redirect(Url.Action("Create", "EventManager", new { Id = photo.EventID }) + "#Photos");
            }
        }
        /// <summary>
        /// To view all photos in an album
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult ViewAlbum(int ID)
        {
            var album = db.PhotoAlbums.Find(ID);
            return View(album);
        }

    }
}