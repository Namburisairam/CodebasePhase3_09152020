using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;


namespace EventManagement.Areas.API.Controllers
{
    public class ExhibitorController : BaseAPIController
    {
        // GET: API/Exhibitor
        public async Task<ActionResult> Index(string Token, string term = "")
        {
            Exhibitors exhibitorBusiness = new Exhibitors();
            var data = await exhibitorBusiness.GetExhibitors(term);

            return Json(new
            {
                data = (await exhibitorBusiness.GetExhibitors(term)).Select(x => new
                {
                    Description = x.Description,
                    DocURL = x.DocURL,
                    ID = x.ID,
                    Name = x.Name,
                    // iconimage = Helpers.ImageToBase64(MvcCustom.APiImagePath(x.UserPackageType.iconimage)),
                    Status = x.Status,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    TwitterURL = x.TwitterURL,
                    WebsiteURL = x.WebsiteURL
                })
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To get all the exhibitors data for specific event
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetExhibitorForSpecificEvent(string Token, int eventId)
        {
            Exhibitors SponsorBusiness = new Exhibitors();
            var data = await SponsorBusiness.GetExhibitorsForSpecificEvent(eventId);

            var result = Json(new
            {
                data = data.Select(x => new
                {
                    x.ID,
                    x.Name,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    //Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                    x.Status,
                    //iconimage = Helpers.ImageToBinary(MvcCustom.APiImagePath(x.UserPackageType.iconimage)),
                    iconimage = imageToUrlConvertionHandler(x.ExhibitorsEvents.FirstOrDefault(y => y.EventID == eventId)?.UserPackageType?.iconimage),
                    x.Description,
                    x.DocURL,
                    x.ExhibitorsEvents.FirstOrDefault(y => y.EventID == eventId)?.BoothNo,
                    Documents = x.Documents.Select(y => new
                    {
                        y.ID,
                        y.DocumentName,
                        FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(y.FilePath))
                    }),
                    x.TwitterURL,
                    x.WebsiteURL,
                })
            }, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }
        /// <summary>
        /// To get Exhibitor data based on event and exhibitorid
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="id"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ExhibitorById(string Token, int id, int eventID)
        {
            Exhibitors attendesBusiness = new Exhibitors();
            var x = await attendesBusiness.GetExhibitorsById(id);
            var eventExhibitor = x.ExhibitorsEvents.FirstOrDefault(y => y.EventID == eventID);
            var floorMapping = eventExhibitor?.FloorMapLocation?.FloorRegionMappings.FirstOrDefault();
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
                    ExhibitorPackageType = x.ExhibitorsEvents.FirstOrDefault(y => y.EventID == eventID)?.UserPackageType?.Name,
                    x.Status,
                    Documents = x.Documents.Select(y => new
                    {
                        y.ID,
                        y.DocumentName,
                        FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(y.FilePath))
                    }),
                    x.TwitterURL,
                    x.WebsiteURL,
                    eventExhibitor?.BoothNo,
                    eventExhibitor?.StartTime,
                    eventExhibitor?.EndTime,
                    Note = x.Notes.FirstOrDefault(y => y.UserID == CurrentUserSession.AttendesID.Value && y.EventID == eventID)?.Text,
                    FloorMap = floorMapping != null ? new
                    {
                        floorMapping?.FloorMapping?.ID,
                        FloorName = eventExhibitor?.FloorMapLocation?.Location
                    } : null
                }
            }, JsonRequestBehavior.AllowGet);
        }
    }
}