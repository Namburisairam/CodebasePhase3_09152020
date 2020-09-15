namespace EventManagement.BusinessLogic.Models
{
    public class SurveyReport
    {
        public string Question { get; set; }

        public string Responses { get; set; }

        public int Count { get; set; }

        public double Average { get; set; }
    }
}
