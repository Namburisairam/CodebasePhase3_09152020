using System.Collections.Generic;
using EventManagement.Areas.API.Models;

namespace EventManagement.BusinessLogic.Models.AttendeesByEventId
{
    public class AttendeesModel : BaseModel
    {
        public List<AttendesDTO> Data { get; set; }

        public AttendeesModel()
        {
            Data = new List<AttendesDTO>();
        }

    }
}
