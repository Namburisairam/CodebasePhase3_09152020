using System.Collections.Generic;
using System.Linq;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class EventVideoManager : BaseLogic
    {
        public IEnumerable<EventVideo> GetEventVideos(int eventID)
        {
            return Db.EventVideos.Where(x => x.EventID == eventID).ToList();
        }
        
        public void AddVideo(EventVideo eventVideo)
        {
            Db.EventVideos.Add(eventVideo);
            Db.SaveChanges();
        }
        
        public void DeleteVideo(int id)
        {
            Db.EventVideos.Remove(Db.EventVideos.Find(id));
            Db.SaveChanges();
        }
    }
}
