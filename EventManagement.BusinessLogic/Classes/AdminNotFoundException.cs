using System;

namespace EventManagement.BusinessLogic.Classes
{
    public class AdminNotFoundException : Exception
    {
        public AdminNotFoundException() : base("No Admin assigned to this event. Please contact Admin")
        {

        }

        public AdminNotFoundException(string message) : base(message)
        {

        }
    }
}
