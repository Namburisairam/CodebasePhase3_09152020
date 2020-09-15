namespace EventManagement.BusinessLogic.Classes
{
    public class IntializeAdminConversation
    {
        public IntializeAdminConversation(int AttendeeID, int eventID)
        {
            this.AttendeeID = AttendeeID;
            EventID = eventID;
        }

        public int AttendeeID { get; }

        public int EventID { get; }

        const string senderIdentity = "CMSAdmin";


    }
}
