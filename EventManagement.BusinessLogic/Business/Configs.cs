using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class Configs : BaseLogic
    {
        public async Task<IEnumerable<Config>> GetAllConfig()
        {
            var records = await Db.Configs.ToListAsync();
            return records;

        }
        
        public async Task<IEnumerable<Notification>> GetAllnoti()
        {
            var records = await Db.Notifications.ToListAsync();
            return records;

        }
        
        public async Task<Config> GetConfigByID(int? id)
        {
            return await Db.Configs.FindAsync(id);
        }
        
        public async Task<bool> AddUpdateConfig(Config savedata)
        {
            if (savedata.ID > 0)
            {
                var updatedata = Db.Configs.Find(savedata.ID);
                updatedata.SupportEmail = savedata.SupportEmail;
                updatedata.EmailBody = savedata.EmailBody;
                updatedata.Subject = savedata.Subject;
                updatedata.ScreenTitle = savedata.ScreenTitle;

                await Db.SaveChangesAsync();


            }
            else
            {
                Db.Configs.Add(savedata);
                await Db.SaveChangesAsync();

            }
            return true;

        }

    }
}
