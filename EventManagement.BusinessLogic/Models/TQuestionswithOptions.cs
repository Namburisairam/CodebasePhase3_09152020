using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models
{
    public class TQuestionswithOptions
    {
        public int QID { get; set; }
       
        public string QuestionText { get; set; }
        
        public int QuestionResponseType { get; set; }
        
        public List<OptionData> options { get; set; }
        //public List<string> options { get; set; }
    }
}
