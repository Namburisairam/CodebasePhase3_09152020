using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Auth
        public ActionResult Index()
        {
            if (CurrentUserSession != null)
                return RedirectToAction("Index", "Home");
            return View();
        }
        /// <summary>
        /// Redirect user to Respective module based on login infomation
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public async Task<ActionResult> Login(FormCollection form)
        {
            try
            {
                AuthLogic logic = new AuthLogic();
                var userSession = await logic.WebLogin(form["email"], form["password"]);
                if (userSession != null)
                {
                    HttpCookie authCookie = new HttpCookie(Literals.CookieToken);
                    authCookie[Literals.APIToken] = userSession.AuthToken;
                    authCookie.Expires = DateTime.Now.AddDays(100);
                    authCookie.Secure = false;
                    Response.Cookies.Add(authCookie);
                    if (userSession.Attende.AdminInfo?.UserRole == (int)enUserRole.Moderator && userSession.IsActive == true)
                        return RedirectToAction("Index", "Moderator");
                    if (userSession.Attende.AdminInfo?.UserRole == (int)enUserRole.EventManager && userSession.IsActive == true)
                        return RedirectToAction("Index", "EventManager");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                Helpers.LogError("Login failed", ex);
                ViewData[Literals.ErrorKey] = ex.Message;
                return RedirectToAction("index");
            }
            return RedirectToAction("index");
        }
        /// <summary>
        /// Logout from existing session
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            //Entities Db = new Entities();
          Request.Cookies[Literals.CookieToken].Expires = DateTime.Now.AddHours(-1);
            //string Auth = Request.Cookies[Literals.CookieToken][Literals.APIToken];
            //var session = Db.UserSessions.FirstOrDefault(x => x.AuthToken == Auth && x.IsActive == true);
            //session.IsActive = false;
            //Db.SaveChanges();
            HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(aCookie);
            }
            return RedirectToAction("index");
        }
    }
}