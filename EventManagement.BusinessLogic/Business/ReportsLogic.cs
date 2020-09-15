using System;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class ReportsLogic : BaseLogic
    {
        Entities db = new Entities();

        public async Task Appdownloads(int UserID)
        {
            Db.AppDownloads.Add(new AppDownload
            {
                UserName = Db.Attendes.Find(UserID).FirstName,
                DownloadDate = DateTime.Now
            });
            await db.SaveChangesAsync();
        }

        public async Task GetAllAttendees()
        {
            db.Attendes.Select(x => x.FirstName).ToList();
        }
    }
}
