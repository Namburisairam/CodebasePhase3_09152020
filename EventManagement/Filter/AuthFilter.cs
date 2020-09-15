using System;
using System.Net;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic.Business;
using TransportERP.Base;

namespace EventManagement.Filter
{
    public class AuthFilterAttribute : ActionFilterAttribute
    {
        public bool IsWebCall { get; set; }
        public enUserRole UserRole { get; set; }

        private string AuthToken { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var baseController = (BaseController)filterContext.Controller;
            if (filterContext.ActionParameters.ContainsKey(Literals.APIToken) && filterContext.ActionParameters[Literals.APIToken] != null)
            {
                AuthToken = filterContext.ActionParameters[Literals.APIToken].ToString();
            }
            if (IsWebCall)
            {
                if (baseController.CurrentUserSession == null)
                {
                    filterContext.Result = new RedirectResult(baseController.Url.Action("Index", "Login"));
                }
                if (UserRole == enUserRole.EventManager)
                {
                    var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    if(!string.Equals(controller, "EventManager", StringComparison.CurrentCultureIgnoreCase))
                        filterContext.Result = new RedirectResult(baseController.Url.Action("Index", "Login"));
                    //filterContext.Result = new RedirectResult(baseController.Url.Action("Index", "EventManager"));
                }
                if (UserRole == enUserRole.Moderator)
                {
                    var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                    if (!string.Equals(controller, "Moderator", StringComparison.CurrentCultureIgnoreCase))
                        //if (controller != "Moderator")
                        filterContext.Result = new RedirectResult(baseController.Url.Action("Index", "Login"));
                    //filterContext.Result = new RedirectResult(baseController.Url.Action("Index", "Moderator"));
                }
                //write logic here
            }
            else if (Authenticate.ValidateToken(AuthToken) == null)
            {
                new AuthLogic().logout(AuthToken);
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                filterContext.Result = new JsonResult { Data = new { error = "Access Denied" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            base.OnActionExecuting(filterContext);
        }
    }
}