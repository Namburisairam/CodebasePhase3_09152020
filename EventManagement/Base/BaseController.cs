using System;
using System.IO;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using EventManagement.DataAccess.DataBase.Model;

namespace TransportERP.Base
{
    public class BaseController : Controller
    {
        UserSession _currentSession = null;

        public string ImageSavePath
        {
            get
            {
                string path = Server.MapPath(Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath));
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
        }

        public string VideoSavePath
        {
            get
            {
                string path = Server.MapPath(Helpers.GetAppSetting(Literals.AppSettingKey_VideoPath));
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
        }

        public string DocumentsSavePath
        {
            get
            {
                string path = Server.MapPath(Helpers.GetAppSetting(Literals.AppSettingKey_DocumentsPath));
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
        }
        public Func<string, string> imageConvertionHandler = x => Helpers.ImageToBase64(MvcCustom.APiImagePath(x));
        public Func<string, string> compressedImageConvertionHandler = x => Helpers.ImageToBase64(Helpers.ResizeImage(Helpers.FileToStream(MvcCustom.APiImagePath(x)), 120));

        public string imageToUrlConvertionHandler(string x)
        {
            return string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, MvcCustom.APiImagePath(x));
        }

        public UserSession CurrentUserSession { get { return _currentSession; } }

        public bool IsAdmin { get { return CurrentUserSession?.Attende?.IsAdmin ?? false; } }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string AuthToken = string.Empty;
            if (filterContext.ActionParameters.ContainsKey(Literals.APIToken) && filterContext.ActionParameters[Literals.APIToken] != null)
            {
                AuthToken = filterContext.ActionParameters[Literals.APIToken].ToString();
            }
            if (string.IsNullOrEmpty(AuthToken) && Request.Cookies[Literals.CookieToken] != null)
                AuthToken = Request.Cookies[Literals.CookieToken][Literals.APIToken];

            if (!string.IsNullOrEmpty(AuthToken))
            {
                AuthLogic authLogic = new AuthLogic();
                _currentSession = authLogic.CheckSession(AuthToken);
                if (_currentSession != null)
                    ViewBag.AppUser = _currentSession.Attende;
                //set current session variale here
            }
            base.OnActionExecuting(filterContext);
        }
    }
}