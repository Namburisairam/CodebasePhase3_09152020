using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventManagement.Controllers
{
    public class SurveyEvent
    {
        public int surveyID { get; set; }
        public string SurveyName { get; set; }
        public int EventID { get; set; }
        public int ActivityID { get; set; }
    }
}