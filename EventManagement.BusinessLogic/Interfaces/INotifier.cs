using System.IO;

namespace EventManagement.BusinessLogic.Interfaces
{
    interface INotifier
    {
        void SendEmail(string fromAddress, params Stream[] stream);
    }
}
