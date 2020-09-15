using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CommonHelpers;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class SponsorsLogic : BaseLogic
    {
        public async Task<List<Sponsor>> GetSponsors(string value = "")
        {
            if (string.IsNullOrEmpty(value))
                return await Db.Sponsors.ToListAsync();
            return await Db.Sponsors.Where(x => x.Name.StartsWith(value) || x.Description.Contains(value)).ToListAsync();
        }
        
        public async Task<Sponsor> GetSponsorById(int? id)
        {
            if (id > 0)
                return await Db.Sponsors.FindAsync(id);
            else
                return new Sponsor();
        }
        
        public async Task<bool> DeleteSponsor(int id)
        {
            // delete logic goes here
            return false;
        }
        
        public async Task<bool> AddUpdateSponsor(Sponsor sponsor, List<int> events)
        {
            try
            {
                if (sponsor.ID > 0)
                {
                    var updateSponsor = await Db.Sponsors.FindAsync(sponsor.ID);
                    updateSponsor.Name = sponsor.Name;
                    updateSponsor.Status = sponsor.Status;
                    updateSponsor.Description = sponsor.Description;
                    //updateSponsor.Packagetype = sponsor.Packagetype;
                    updateSponsor.DocURL = sponsor.DocURL;
                    updateSponsor.WebsiteURL = sponsor.WebsiteURL;
                    if (!string.IsNullOrEmpty(sponsor.Thumbnail))
                        updateSponsor.Thumbnail = sponsor.Thumbnail;

                    if (sponsor.MarkDocumentasFavourite != null)
                        updateSponsor.MarkDocumentasFavourite = sponsor.MarkDocumentasFavourite;
                    else
                        updateSponsor.MarkDocumentasFavourite = false;

                    if (sponsor.SaveShareDocuments != null)
                        updateSponsor.SaveShareDocuments = sponsor.SaveShareDocuments;
                    else
                        updateSponsor.SaveShareDocuments = false;

                    if (events == null)
                    {
                        var removeAll = Db.SponsorsEvents.Where(x => x.SponsorID == sponsor.ID);
                        Db.SponsorsEvents.RemoveRange(removeAll);
                        await Db.SaveChangesAsync();
                    }
                    else
                    {
                        var removeEvent = Db.SponsorsEvents.Where(x => x.SponsorID == sponsor.ID && !events.Contains(x.EventID));
                        if (removeEvent.Count() > 0)
                        {
                            Db.SponsorsEvents.RemoveRange(removeEvent);
                            await Db.SaveChangesAsync();
                        }
                        foreach (var item in events)
                        {
                            if (!Db.SponsorsEvents.Any(x => x.EventID == item && x.SponsorID == sponsor.ID))
                            {
                                Db.SponsorsEvents.Add(new SponsorsEvent
                                {
                                    EventID = item,
                                    SponsorID = sponsor.ID
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
                        sponsor.SponsorsEvents = events.Select(x => new SponsorsEvent { EventID = x }).ToList();
                    Db.Sponsors.Add(sponsor);
                    await Db.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        
        public List<Sponsor> GetSponsorsForSpecificEvent(int EventId)
        {
            bool isSponsorNameSort = Db.Events.FirstOrDefault(x => x.ID == EventId).EnableSponsorNameSort;
            bool isSponsorNameAscendingSort = Db.Events.FirstOrDefault(x => x.ID == EventId).SponsorSortBy;

            bool isSponsorTypeSort = Db.Events.FirstOrDefault(x => x.ID == EventId).EnableSponsorTypeSort;
            bool isSponsorTypeAscendingSort = Db.Events.FirstOrDefault(x => x.ID == EventId).SponsorTypeSortOrder;

            if (isSponsorNameSort && isSponsorTypeSort)
            {
                if (isSponsorNameAscendingSort && isSponsorTypeAscendingSort)
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => Helpers.HtmlToString(z.Name)).ThenBy(a => a.SponsorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else if (isSponsorNameAscendingSort && !isSponsorTypeAscendingSort)
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => Helpers.HtmlToString(z.Name)).ThenByDescending(a => a.SponsorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else if (!isSponsorNameAscendingSort && isSponsorTypeAscendingSort)
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => Helpers.HtmlToString(z.Name)).ThenBy(a => a.SponsorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => Helpers.HtmlToString(z.Name)).ThenByDescending(a => a.SponsorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
            }
            else if (isSponsorNameSort)
            {
                if (isSponsorNameAscendingSort)
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => Helpers.HtmlToString(z.Name)).ToList();
                }
                else
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => Helpers.HtmlToString(z.Name)).ToList();
                }
            }
            else if (isSponsorTypeSort)
            {
                if (isSponsorTypeAscendingSort)
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => z.SponsorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else
                {
                    return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => z.SponsorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
            }

            else
            {
                return Db.Sponsors.Where(x => x.SponsorsEvents.Any(y => y.EventID == EventId)).ToList();
            }
        }
        
        public async Task<int> SponsorQrCode(int id, string qrCode, int userId, string description, string url)
        {
            var code = new QRCode
            {
                SponsorID = id,
                GeneratedBY = userId,
                GeneratedON = DateTime.Now,
                Description = string.Empty,
                Status = true,
                WebURL = url ?? string.Empty,
                CodePath = qrCode
            };
            Db.QRCodes.Add(code);
            await Db.SaveChangesAsync();
            return code.ID;
        }
        
        public async Task<int> UpdateSponsorQrCode(int id, string qrCode)
        {
            var data = await Db.QRCodes.FindAsync(id);
            data.CodePath = qrCode;
            await Db.SaveChangesAsync();

            return 0;
        }

    }
}

