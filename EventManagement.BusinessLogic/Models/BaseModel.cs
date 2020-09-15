using System.Net;

namespace EventManagement.Areas.API.Models
{
    public class BaseModel
    {
        public HttpStatusCode Status { get; set; }

        public string Message { get; set; }

        public BaseModel()
        {
            Status = HttpStatusCode.OK;
            Message = string.Empty;
        }
    }
}