using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models
{
    public class SurveyQuestions
    {
        public int QID { get; set; }

        public string QuestionText { get; set; }

        public int QuestionResponseType { get; set; }
        
        public List<OptionData> options { get; set; }
    }
}
