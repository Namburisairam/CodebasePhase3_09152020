using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Classes;



namespace EventManagement.Areas.API.Controllers
{
    public class SpeakerController : BaseAPIController
    {
        // GET: API/Speaker
        //public async Task<ActionResult> Index(string Token, string term = "")
        //{
        //    ////Speakers SpeakerBusiness = new Speakers();
        //    //var data = await SpeakerBusiness.GetSpeakers_(term);

        //    //return Json(new
        //    //{
        //    //    data = (await SpeakerBusiness.GetSpeakers_(term)).Select(x => new
        //    //    {
        //    //        Description = x.Description,
        //    //        DocURL = x.DocURL,
        //    //        ID = x.ID,
        //    //        Name = x.FirstName + x.LastName,
        //    //        iconimage = Helpers.ImageToBase64(MvcCustom.APiImagePath(x.UserPackageType.iconimage)),
        //    //        Status = x.Status,
        //    //        Thumbnail = Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
        //    //        LinkedinURL = x.LinkedIN,
        //    //        FacebookURL = x.Facebook,
        //    //        TwitterURL = x.TwitterURL,
        //    //    })
        //    //}, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public async Task<ActionResult> GetSpeakerForSpecificEvent(string Token, int eventId)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var data = await attendesBusiness.GetAttendeeForSpecificEvent(eventId, new SpeakerValidator());
            return Json(data.Select(x => new
            {
                x.ID,
                x.FirstName,
                x.Description,
                Thumbnail = imageToUrlConvertionHandler(x.Thumbnail), // Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
                x.FacebookURL,
                x.TwitterURL,
                x.InstagramURL,
                x.Status,
                x.Email,
                x.LinkedinURL,
                x.Lastname,
                x.TitleName,
                x.CompanyName,
                x.EnableMessaging
            }), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get information about specified speaker
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="id"></param>
        /// <returns>returns speaker information</returns>
        [HttpGet]
        public async Task<ActionResult> SpeakerById(string Token, int id)
        {
            AttendesLogic attendesBusiness = new AttendesLogic();
            var x = await attendesBusiness.GetAttendesById(id);
            return Json(new
            {
                data = new
                {
                    x.ID,
                    x.FirstName,
                    x.Lastname,
                    x.Description,
                    Thumbnail = imageToUrlConvertionHandler(x.Thumbnail),
                    x.Status,
                    x.TitleName,
                    x.CompanyName,
                    Documents = x.Documents.Select(y => new
                    {
                        y.ID,
                        y.DocumentName,
                        FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(y.FilePath))
                    }),
                    Activities = x.BookMarks.Select(y => new
                    {
                        y.Activite.ID,
                        y.Activite.Name,
                        y.Activite.StartTime,
                        y.Activite.EndTime,
                        Thumbnail = imageToUrlConvertionHandler(y.Activite.Thumbnail)
                    }),
                    x.LinkedinURL,
                    x.FacebookURL,
                    x.TwitterURL,
                    x.Website,
                    x.InstagramURL,
                    x.EnableMessaging
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // not used
        //public ActionResult GetSpeakersByActivityID(string Token, int activityID) {
        //    Speakers attendesBusiness = new Speakers();
        //    var speakersByActivity = attendesBusiness.GetSpeakersByActivity(activityID);
        //    return Json(new
        //    {
        //        data = speakersByActivity.Select(x => new
        //        {
        //            x.FirstName,
        //            x.LastName,
        //            Thumbnail = Helpers.ImageToBase64(MvcCustom.APiImagePath(x.Thumbnail)),
        //            x.Status,
        //            iconimage = Helpers.ImageToBase64(MvcCustom.APiImagePath(x.UserPackageType.iconimage)),
        //            x.Description,
        //            x.DocURL,
        //            x.TitleName,
        //            x.CompanyName,
        //            x.MarkDocumentasFavourite,
        //            x.SaveShareDocuments,
        //            Documents = x.Documents.Select(y => new
        //            {
        //                y.ID,
        //                y.DocumentName,
        //                FilePath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.ApiDocumentPath(y.FilePath))
        //            }),
        //            x.LinkedIN,
        //            x.Facebook,
        //            x.TwitterURL
        //        })
        //    }, JsonRequestBehavior.AllowGet);
        //}

    }
}