using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;

namespace EventManagement.Areas.API.Controllers
{
    public class VendorsController : BaseAPIController
    {
        // GET: API/Vendors
        public async Task<ActionResult> Index(string Token, string term = "")
        {
            VendorsLogic VendorBusiness = new VendorsLogic();
            var data = await VendorBusiness.GetVendor(term);

            return Json(new
            {
                data = (await VendorBusiness.GetVendor(term)).Select(x => new
                {
                    Description = x.Description,
                    DocURL = x.DocURL,
                    ID = x.ID,
                    Name = x.Name,
                    //Package = x.UserPackageType.Name,
                    Status = x.Status,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    TwitterURL = x.TwitterURL,
                    WebsiteURL = x.WebsiteURL,
                })
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get vendors information for specific event
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns>Returns vendors data</returns>
        [HttpGet]
        public async Task<ActionResult> GetVendorForSpecificEvent(string Token, int eventId)
        {
            VendorsLogic SponsorBusiness = new VendorsLogic();
            var data = await SponsorBusiness.GetVendorForSpecificEvent(eventId);
            return Json(new
            {
                data = data.Select(x => new
                {
                    x.ID,
                    x.Name,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.Status,
                    x.Description,
                    x.DocURL,
                    iconimage = imageToUrlConvertionHandler(x.VendorsEvents.FirstOrDefault(y => y.EventID == eventId)?.UserPackageType?.iconimage),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.VendorsEvents.FirstOrDefault(y => y.EventID== eventId)?.UserPackageType?.iconimage)),
                    x.TwitterURL,
                    x.WebsiteURL,
                    VendorPackageType = x.VendorsEvents.FirstOrDefault(y => y.EventID == eventId)?.UserPackageType?.Name,
                    x.VendorsEvents.FirstOrDefault(y => y.EventID == eventId)?.BoothNo,
                })
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get vendor information for specific event and id
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="id"></param>
        /// <param name="eventID"></param>
        /// <returns>Returns vendor information</returns>
        [HttpGet]
        public async Task<ActionResult> VendorById(string Token, int id, int eventID)
        {
            VendorsLogic attendesBusiness = new VendorsLogic();
            EventLogic eventLogic = new EventLogic();
            var x = await attendesBusiness.GetVendorById(id);
            var vendorEvent = x.VendorsEvents.FirstOrDefault(y => y.EventID == eventID);
            var VendorQuestions = await eventLogic.VendorSurveyQuestions(CurrentUserSession.AttendesID.Value, id);
            var floorMapping = vendorEvent?.FloorMapLocation?.FloorRegionMappings.FirstOrDefault();
            return Json(new
            {
                data = new
                {
                    x.ID,
                    x.Name,
                    x.Description,
                    x.DocURL,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.Status,
                    x.TwitterURL,
                    x.WebsiteURL,
                    vendorEvent?.BoothNo,
                    vendorEvent?.StartTime,
                    VendorPackageType = x.VendorsEvents.FirstOrDefault(y => y.EventID == eventID)?.UserPackageType?.Name,
                    vendorEvent?.EndTime,
                    SurveyQuestions = VendorQuestions.Select(a => new QuestionsWithOptions
                    {
                        QID = a.QID,
                        QuestionText = a.QuestionText,
                        IsAtActiivtyLevel = a.IsAtActiivtyLevel,
                        QuestionResponseType = a.QuestionResponseType,
                        HideorShowQuestion = a.HideorShowQuestion,
                        options = a.options,
                        IsSubmitted = a.IsSubmitted,
                        IsAtVendorLevel = a.IsAtVendorLevel,
                        IsAtSponsorLevel = a.IsAtSponsorLevel
                    }),
                    Note = x.Notes.FirstOrDefault(y => y.UserID == CurrentUserSession.AttendesID.Value && y.EventID== eventID)?.Text,
                    FloorMap = floorMapping != null ? new
                    {
                        floorMapping?.FloorMapping?.ID,
                        FloorName = vendorEvent?.FloorMapLocation?.Location
                    } : null,
                    Activities = x.VendorActivities.Select(y => y.Activite).Select(y => new { y.ID, y.Name, y.StartTime, y.EndTime }),
                    Vendors = x.VendorActivities.SelectMany(y => y.Activite.BookMarks.Select(z => z.Attende))
                                .Where(y => y.IsSpeaker).Distinct(new AttendeeComparer()).
                                Select(y => new
                                {
                                    y.FirstName,
                                    y.Lastname,
                                    y.TitleName,
                                    Thubnail = compressedImageConvertionHandler(y.Thumbnail)
                                })
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: API/Vendors/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: API/Vendors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: API/Vendors/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: API/Vendors/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: API/Vendors/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: API/Vendors/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: API/Vendors/Delete/5
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
    }
}
