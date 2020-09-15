using System.Collections.Generic;

namespace EventManagement.BusinessLogic.Models
{
    public class TemplateModel
    {
        public string TemplateName { get; set; }
        
        public string TemplateDescription { get; set; }
        
        public string  TemplateLevel { get; set; }
        
        public int? EventID { get; set; }
        
        public int? ActivityID { get; set; }
        
        public List<TemplateQuestions> QuestionsOptions { get; set; }
    }
}
