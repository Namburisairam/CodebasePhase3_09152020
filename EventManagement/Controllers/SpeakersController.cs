using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.Filter;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    [AuthFilter(IsWebCall = true)]
    public class SpeakersController : BaseController
    {
        //Speakers speakerLogic = new Speakers();
        EventLogic eventlogic = new EventLogic();
        AttendesLogic attendBuisness = new AttendesLogic();
        // GET: Speakers
        public async Task<ActionResult> Index()
        {
            return View(await attendBuisness.GetAttendes(checkAttendeeValidity: new SpeakerValidator()));
        }

        // GET: Speakers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attende attende = await attendBuisness.GetAttendee(id.Value);
            if (attende == null)
            {
                return HttpNotFound();
            }
            return View(attende);
        }

        /// <summary>
        /// to get all speakers
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Speakers/Create
        public async Task<ActionResult> Create(int? id)
        {
            var attende = await attendBuisness.GetAttendee(id);
            attende.IsSpeaker = true;
            ViewBag.Events = await eventlogic.GetAllEvents();
            return View(attende);
        }
        /// <summary>
        /// To create a new speaker in specified event
        /// </summary>
        /// <param name="speaker"></param>
        /// <param name="events"></param>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
        // POST: Speakers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        //public async Task<ActionResult> Create(Speakers_ speaker, List<int> events, HttpPostedFileBase thumbnail)
        //{
        //    try
        //    {
        //        string fileName = "";
        //        if (thumbnail != null)
        //            fileName = Guid.NewGuid().ToString() + ".png";
        //        speaker.Thumbnail = fileName;
        //        //await speakerLogic.AddUpdateSpeakers_(speaker, events);
        //        if (thumbnail != null)
        //            thumbnail.SaveAs(Path.Combine(ImageSavePath, fileName));
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        Helpers.LogError("Speaker Error", ex);
        //        EventLogic eventLogic = new EventLogic();
        //        ViewBag.Events = await eventLogic.GetAllEvents();
        //        ViewBag.Error = Literals.ErrorMessage;
        //        return View(speaker);
        //    }
        //}



        // GET: Speakers/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Speakers/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //public async Task<ActionResult> DeleteSpeaker(int id)
        //{
        //    Entities db = new Entities();
        //    //Speakers_ speaker = await db.Speakers_.FindAsync(id);
        //    //var Sperec = db.SpeakersEvents.Where(x => x.SpeakerID == speaker.ID).ToList();
        //    //foreach (var item in Sperec)
        //    //{
        //    //    db.SpeakersEvents.Remove(item);
        //    //    await db.SaveChangesAsync();
        //    //}
        //    //db.Speakers_.Remove(speaker);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}
