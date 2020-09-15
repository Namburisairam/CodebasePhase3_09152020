using System.Collections.Generic;
using EventManagement.Areas.API.Models;

namespace EventManagement.BusinessLogic.Models
{
    public class SponsorsModel : BaseModel
    {
        public List<SponsorsDTO> Data { get; set; }

        public SponsorsModel()
        {
            Data = new List<SponsorsDTO>();
        }

    }
}
