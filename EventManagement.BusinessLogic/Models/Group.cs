namespace EventManagement.BusinessLogic.Models
{
    public class Group
    {
        public int ID { get; }
        
        public string Name { get; }
        
        public bool IsUserGroup { get; }

        public Group(int iD, string name, bool isUserGroup)
        {
            ID = iD;
            Name = name;
            IsUserGroup = isUserGroup;
        }
    }
}
