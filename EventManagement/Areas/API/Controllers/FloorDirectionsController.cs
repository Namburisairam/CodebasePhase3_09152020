using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;
using EventManagement.Filter;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EventManagement.Areas.API.Controllers
{
    [AuthFilter]
    public class FloorDirectionsController : BaseAPIController
    {
        FloorDirectionsLogic floorDirectionsLogic = new FloorDirectionsLogic();
        /// <summary>
        /// To get floor directions for given event
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public ActionResult GetEventFloorDirections(string Token, int eventID)
        {
            List<FloorMap> eventFloorMaps = floorDirectionsLogic.GetEventFloorMaps(eventID, imageConvertionHandler);
            foreach(var item in eventFloorMaps)
            {
                item.PhotoURL = imageToUrlConvertionHandler(item.PhotoURL);
            }
            return Json(eventFloorMaps, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get floormap information based on event and floormapid
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="eventID"></param>
        /// <param name="FloorMapID"></param>
        /// <returns></returns>
        public ActionResult GetFloorMapByID(string Token, int eventID, int FloorMapID)
        {
            FloorMap floorMap = floorDirectionsLogic.GetFloorMapByID(FloorMapID, eventID, imageConvertionHandler);
            floorMap.PhotoURL = imageToUrlConvertionHandler(floorMap.PhotoURL);
            return Json(floorMap, JsonRequestBehavior.AllowGet);
        }

    }
}