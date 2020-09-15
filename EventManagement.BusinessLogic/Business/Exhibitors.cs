using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class Exhibitors : BaseLogic
    {
        public async Task<List<Exhibitor>> GetExhibitors(string value = "")
        {
            if (string.IsNullOrEmpty(value))
                return await Db.Exhibitors.ToListAsync();
            return await Db.Exhibitors.Where(x => x.Name.StartsWith(value) || x.Description.Contains(value)).ToListAsync();
        }
        
        public async Task<Exhibitor> GetExhibitorsById(int? id)
        {
            if (id > 0)
                return await Db.Exhibitors.FindAsync(id);
            else
                return new Exhibitor();
        }
        
        public async Task<bool> DeleteExhibitors(int id)
        {
            // delete logic goes here
            return false;
        }
        
        public async Task<bool> AddUpdateExhibitors(Exhibitor Exhibitors, List<int> events)
        {
            if (Exhibitors.ID > 0)
            {
                var updateExhibitors = await Db.Exhibitors.FindAsync(Exhibitors.ID);
                updateExhibitors.Name = Exhibitors.Name;
                updateExhibitors.Status = Exhibitors.Status;
                //updateExhibitors.Packagetype = Exhibitors.Packagetype;
                updateExhibitors.Description = Exhibitors.Description;
                updateExhibitors.DocURL = Exhibitors.DocURL;
                updateExhibitors.TwitterURL = Exhibitors.TwitterURL;
                updateExhibitors.WebsiteURL = Exhibitors.WebsiteURL;
                if (Exhibitors.MarkDocumentasFavourite != null)
                    updateExhibitors.MarkDocumentasFavourite = Exhibitors.MarkDocumentasFavourite;
                else
                    updateExhibitors.MarkDocumentasFavourite = false;

                if (Exhibitors.SaveShareDocuments != null)
                    updateExhibitors.SaveShareDocuments = Exhibitors.SaveShareDocuments;
                else
                    updateExhibitors.SaveShareDocuments = false;
                if (!string.IsNullOrEmpty(Exhibitors.Thumbnail))
                    updateExhibitors.Thumbnail = Exhibitors.Thumbnail;

                if (events == null)
                {
                    var removeAll = Db.ExhibitorsEvents.Where(x => x.ExhibitorsID == Exhibitors.ID);
                    Db.ExhibitorsEvents.RemoveRange(removeAll);
                    await Db.SaveChangesAsync();
                }
                else
                {
                    var removeEvent = Db.ExhibitorsEvents.Where(x => x.ExhibitorsID == Exhibitors.ID && !events.Contains(x.EventID.Value));
                    if (removeEvent.Count() > 0)
                    {
                        Db.ExhibitorsEvents.RemoveRange(removeEvent);
                        await Db.SaveChangesAsync();
                    }
                    foreach (var item in events)
                    {
                        if (!Db.ExhibitorsEvents.Any(x => x.EventID == item && x.ExhibitorsID == Exhibitors.ID))
                        {
                            Db.ExhibitorsEvents.Add(new ExhibitorsEvent
                            {
                                EventID = item,
                                ExhibitorsID = Exhibitors.ID
                            });
                        }
                    }
                    await Db.SaveChangesAsync();
                }

                await Db.SaveChangesAsync();

            }
            else
            {
                if (events != null && events.Count > 0)
                    Exhibitors.ExhibitorsEvents = events.Select(x => new ExhibitorsEvent { EventID = x }).ToList();
                Db.Exhibitors.Add(Exhibitors);
                await Db.SaveChangesAsync();
            }
            return true;
        }
        
        public async Task<List<Exhibitor>> GetExhibitorsForSpecificEvent(int EventId)
        {
            return await Db.Exhibitors.Where(x => x.ExhibitorsEvents.Any(y => y.EventID == EventId)).Include(x => x.Documents).OrderBy(x => x.Name).ToListAsync();
        }

    }
}
