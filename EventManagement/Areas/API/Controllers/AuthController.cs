using CommonHelpers;
using EventManagement.Base;
using EventManagement.BusinessLogic.Business;
using EventManagement.DataAccess.DataBase.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Models;
using Newtonsoft.Json;
using EventManagement.BusinessLogic.Models.APIDTOOjbects;
using System.Net;

namespace EventManagement.Areas.API.Controllers
{
    public class AuthController : BaseAPIController
    {
        AuthLogic authLogic;
        Notifications notifications = new Notifications();

        /// <summary>
        /// Login data for attendee based on email and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="type"></param>
        /// <param name="token"></param>
        /// <param name="isactive"></param>
        /// <returns>Returns attendee information for given email and password</returns>
        // GET: API/Authe
        public async Task<ActionResult> Login(string email, string password, int type, string token, string isactive, string mobileOSVersion, string mobileDeviceUniqueToken)
        {
            try
            {

                authLogic = new AuthLogic();
                var data = await authLogic.AttendiLogin(email, password, (enPlatformTypes)type, token, isactive, compressedImageConvertionHandler, ImageSavePath, mobileOSVersion, mobileDeviceUniqueToken);
                if (!data.IsAuthenticated)
                {
                    //Response.StatusCode = 401;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (HttpException ex)
            {
                GPSAPILogin1 accessEvents = new GPSAPILogin1();
                accessEvents.IsAuthenticated = false;
                if (ex.GetHttpCode() == 401)
                {
                    accessEvents.Message = "Username or Password is not correct";
                    //Response.StatusCode = 401;
                    return Json(accessEvents, JsonRequestBehavior.AllowGet);
                }
                accessEvents.Message = "Please contact Admin";
                return Json(accessEvents, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetAccessToken(string email, string password, int type, string token, string isactive, string mobileOSVersion, string mobileDeviceUniqueToken)
        {
            try
            {
                authLogic = new AuthLogic();
                var data = await authLogic.AttendiLogin(email, password, (enPlatformTypes)type, token, isactive, compressedImageConvertionHandler, ImageSavePath, mobileOSVersion, mobileDeviceUniqueToken);

                string newToken = data.Token;
                bool status = string.IsNullOrEmpty(data.Message);
                string message = string.IsNullOrEmpty(data.Message) ? string.Empty : data.Message;
                var newTokenObj = new { Token = newToken, Status = status, Message = message };

                return Json(newTokenObj, JsonRequestBehavior.AllowGet);

            }
            catch (HttpException ex)
            {
                string newToken = string.Empty;
                string message = ex.GetHttpCode() == 401 ? "Username or Password is not correct" : "Please contact Admin";
                var newTokenObj = new { Token = newToken, Status = false, Message = message };
                return Json(newTokenObj, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Authenticate email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Returns whether given email is valid or not</returns>
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> VerifyEmail(string email)
        {
            authLogic = new AuthLogic();
            var response = await authLogic.AttendeVerifyEmail(email);
            return Json(response);
        }
        /// <summary>
        ///  update password for given email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>update password for given email</returns>
        public async Task<ActionResult> UpdatePassword(string email, string password)
        {
            authLogic = new AuthLogic();
            var response = await authLogic.UpdateAttendePassword(email, password);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VerifyCode()
        {
            return Json(false);
        }

        public ActionResult SaveImage(HttpPostedFileBase saveFile)
        {
            saveFile.SaveAs("");
            return Json("Succes");
        }
        public async Task<ActionResult> GetConfiguration()
        {
            Configs configlogic = new Configs();
            var response = await configlogic.GetAllConfig();
            return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
        }
        //public async Task<ActionResult> DeleteNotification(string token, int id)
        //{
        //    Entities db = new Entities();
        //    try
        //    {
        //        var response = await db.Notifications.FindAsync(id);
        //        db.Notifications.Remove(response);
        //        await db.SaveChangesAsync();
        //        return Json(new { Data = "OK" }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //}
        //public async Task<ActionResult> DeleteAllNotification(string token)
        //{
        //    Entities db = new Entities();
        //    try
        //    {
        //        var response = await db.Notifications.Where(x => x.AttendesID == CurrentUserSession.AttendesID.Value).ToListAsync();
        //        foreach (var item in response)
        //        {
        //            db.Notifications.Remove(item);
        //            await db.SaveChangesAsync();

        //        }

        //        return Json(new { Data = "OK" }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}
        /// <summary>
        /// Used to get notifications of a specific event
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventid"></param>
        /// <returns>Returns notifications data of specified event</returns>
        public async Task<ActionResult> GetNotifications(string token, int eventid)
        {

            Entities db = new Entities();
            var response = notifications.GetAllUserNotificationsForEvent(CurrentUserSession.AttendesID.Value, eventid);
            return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Gets notifications which are unread
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventid"></param>
        /// <returns>returns notifications data which is not yet read</returns>
        public async Task<ActionResult> GetUnreadNotifications(string token, int eventid)
        {

            Entities db = new Entities();
            var Last20days = DateTime.Today.AddDays(-20);
            var response = notifications.GetUnreadUserNotificationsForEvent(CurrentUserSession.AttendesID.Value, eventid);

            return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Gets count of notifications which are not read
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventid"></param>
        /// <returns>returns unread notifications count</returns>
        public async Task<ActionResult> GetUnreadNotificationsCount(string token, int eventid)
        {
            Entities db = new Entities();
            var res = notifications.GetUnreadUserNotificationsForEvent(CurrentUserSession.AttendesID.Value, eventid).Count();
            return Json(new { Data = res }, JsonRequestBehavior.AllowGet);
        }
        public DateTime StringToDate(string _strDateArg)
        {
            string[] strDate = _strDateArg.Split('/');
            DateTime dt = new DateTime(Convert.ToInt32(strDate[2]), Convert.ToInt32(strDate[0]), Convert.ToInt32(strDate[1]));
            return dt;
        }
        /// <summary>
        /// Uodates status to read if notifications are read
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ids"></param>
        /// <param name="status"></param>
        /// <returns>returns status</returns>
        public async Task<ActionResult> ReadNotifications(string token, string ids, bool status)
        {
            try
            {
                Entities db = new Entities();
                string[] strids = ids.Split(',');
                if (status)
                {
                    notifications.ReadNotifications(CurrentUserSession.AttendesID.Value, Convert.ToInt32(ids));
                }
                else
                {
                    notifications.UnReadNotifications(CurrentUserSession.AttendesID.Value, Convert.ToInt32(ids));
                }
                return Json(new { Data = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        /// <summary>
        /// Logs out the existing session
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns status</returns>
        public async Task<ActionResult> logout(string token)
        {

            if (token != null && token != "")
            {
                Entities Db = new Entities();
                var check = Db.UserSessions.Where(x => x.AuthToken == token && x.IsActive == true).FirstOrDefault();
                check.IsActive = false;
                await Db.SaveChangesAsync();
                return Json(new { Data = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Data = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public async Task<ActionResult> ForgotPassword(string email)
        {
            try
            {
                AuthLogic a = new AuthLogic();
                var returnCode = a.GenerateCode(email);
                //authLogic.GenerateCode(a);
                string[] strMessageReturn = returnCode.Split('|');
                if (strMessageReturn[0] == "success")
                {
                    var ResultSignup = new
                    {
                        message = strMessageReturn[0],
                        code = strMessageReturn[1],
                        codeDate = strMessageReturn[2]
                    };
                    return Json(ResultSignup, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(strMessageReturn[0], JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

}