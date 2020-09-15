#region Libraries
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;
#endregion

namespace EventManagement.BusinessLogic.Business
{

    #region Activity Logic Class
    public class ActivityLogic : BaseLogic
    {

        /// <summary>
        /// GetActivities Method
        /// </summary>
        /// <returns>
        /// List Of Activities
        /// </returns>
        public async Task<List<Activite>> GetActivities()
        {
            return await Db.Activites.ToListAsync();
        }

        /// <summary>
        /// Get Activity By Id Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Activite
        /// </returns>

        public async Task<Activite> GetActivity(int? id)
        {
            if (id > 0)
                return await Db.Activites.FindAsync(id);
            return new Activite();
        }

        public async Task<IEnumerable<Attende>> getActivityAttendees(int id)
        {
            return await Db.BookMarks.Where(x => x.ActivityID == id).Select(x => x.Attende).ToListAsync();
        }
    }
    #endregion

}





