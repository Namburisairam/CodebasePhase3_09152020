using CommonHelpers;
using System.IO;

namespace System.Web.Mvc
{
    public static class MvcCustom
    {
        public static string ImagePath(string imageName)
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            if (string.IsNullOrEmpty(imageName))
                return UrlHelper.GenerateContentUrl("~/Content/NoImage.png", httpContext);
            var file = Path.Combine(CommonHelpers.Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath), imageName);
            return UrlHelper.GenerateContentUrl(file, httpContext);
        }
        
        public static string APiImagePath(string imageName)
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            if (string.IsNullOrEmpty(imageName))

                return UrlHelper.GenerateContentUrl("/ImageFile/NoImage.png", httpContext);
            var file = Path.Combine(CommonHelpers.Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath), imageName);
            var fullname = string.Format("{0}", file);
            return UrlHelper.GenerateContentUrl(fullname, httpContext);
        }

        public static string ApiDocumentPath(string docPath)
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            if (string.IsNullOrEmpty(docPath))
            {
                return null;
            }
            var file = Path.Combine(CommonHelpers.Helpers.GetAppSetting(Literals.AppSettingKey_DocumentsPath), docPath);
            var fullname = string.Format("{0}", file);
            return UrlHelper.GenerateContentUrl(fullname, httpContext);
        }
        
        public static string ReturnImagePath()
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            return UrlHelper.GenerateContentUrl("~/ImageFile/", httpContext);
        }

        public static string RootImagePath()
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            return UrlHelper.GenerateContentUrl(CommonHelpers.Helpers.GetAppSetting(Literals.AppSettingKey_ImagePath), httpContext);
        }
    }
}