namespace EventManagement.BusinessLogic.Models
{
    public class DocumentModel
    {
        public int ID { get; set; }

        public int DocumentID { get; set; }
        
        public string FilePath { get; set; }
        
        public string MappedType { get; set; }

        public string AssignedTo { get; set; }

        public string DocumentName { get; set; }
    }
}
