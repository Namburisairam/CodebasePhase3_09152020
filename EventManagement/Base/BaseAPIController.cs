using System.Net;
using System.Web.Mvc;
using CommonHelpers;
using EventManagement.BusinessLogic;
using EventManagement.BusinessLogic.Business;
using TransportERP.Base;

namespace EventManagement.Base
{
    public class BaseAPIController : BaseController
    {
        public const string ErrorMessage = "An unknown error has occurred , Please contact your system administrator";

        
        protected new JsonResult Json(object data)
        {
            var jsonResult = base.Json(data);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        protected new JsonResult Json(object data, JsonRequestBehavior jsonRequestBehavior)
        {
            var jsonResult = base.Json(data, jsonRequestBehavior);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Helpers.LogError("API Error: ", filterContext.Exception);
            var errorService = new Error();
            errorService.LogErrorToDB(filterContext.Exception, "Api");
            APIException apiexception = filterContext.Exception as APIException;
            string message = string.Empty + filterContext.Exception;
            if (apiexception != null)
                message = apiexception.Message;
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            filterContext.Result = new JsonResult
            {
                Data = new { error = ErrorMessage }
                ,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            filterContext.ExceptionHandled = true;
            base.OnException(filterContext);
        }
    }
}