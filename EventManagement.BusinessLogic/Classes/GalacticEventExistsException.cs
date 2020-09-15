using System;

namespace EventManagement.BusinessLogic.Classes
{
    public class GalacticEventExistsException : Exception
    {
        public GalacticEventExistsException()
        {

        }

        public GalacticEventExistsException(string message) : base(message)
        {

        }

    }
}
