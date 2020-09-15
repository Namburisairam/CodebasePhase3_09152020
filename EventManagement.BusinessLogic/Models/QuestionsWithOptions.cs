using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models
{
    public class QuestionsWithOptions
    {
        public int QID { get; set; }
        
        public string QuestionText { get; set; }
        
        public bool IsAtActiivtyLevel { get; set; }
        
        public string QuestionResponseType { get; set; }
        
        public bool? HideorShowQuestion { get; set; }
        
        public List<string> options { get; set; }
        
        public bool IsSubmitted { get; set; }

        public bool IsAtVendorLevel { get; set; }

        public bool IsAtSponsorLevel { get; set; }

    }
}
