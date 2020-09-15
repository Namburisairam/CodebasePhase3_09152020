using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CommonHelpers;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class VendorsLogic : BaseLogic
    {
        public async Task<List<Vendor>> GetVendor(string value = "")
        {
            if (string.IsNullOrEmpty(value))
                return await Db.Vendors.ToListAsync();
            return await Db.Vendors.Where(x => x.Name.StartsWith(value) || x.Description.Contains(value)).ToListAsync();
        }
        
        public async Task<Vendor> GetVendorById(int? id)
        {
            if (id > 0)
                return await Db.Vendors.FindAsync(id);
            else
                return new Vendor();
        }
        
        public async Task<bool> DeleteVenndor_(int id)
        {
            // delete logic goes here
            return false;
        }
        
        public async Task<bool> AddUpdateVendor(Vendor vendor, List<int> events)
        {
            if (vendor.ID > 0)
            {
                var updateVendor = await Db.Vendors.FindAsync(vendor.ID);
                updateVendor.Name = vendor.Name;
                updateVendor.Status = vendor.Status;
                //updateVendor.Packagetype = vendor.Packagetype;
                updateVendor.Description = vendor.Description;
                updateVendor.DocURL = vendor.DocURL;
                updateVendor.TwitterURL = vendor.TwitterURL;
                updateVendor.WebsiteURL = vendor.WebsiteURL;
                if (!string.IsNullOrEmpty(vendor.Thumbnail))
                    updateVendor.Thumbnail = vendor.Thumbnail;

                if (events == null)
                {
                    var removeAll = Db.VendorsEvents.Where(x => x.VendorsID == vendor.ID);
                    Db.VendorsEvents.RemoveRange(removeAll);
                    await Db.SaveChangesAsync();
                }
                else
                {
                    var removeEvent = Db.VendorsEvents.Where(x => x.VendorsID == vendor.ID && !events.Contains(x.EventID.Value));
                    if (removeEvent.Count() > 0)
                    {
                        Db.VendorsEvents.RemoveRange(removeEvent);
                        await Db.SaveChangesAsync();
                    }
                    foreach (var item in events)
                    {
                        if (!Db.VendorsEvents.Any(x => x.EventID == item && x.VendorsID == vendor.ID))
                        {
                            Db.VendorsEvents.Add(new VendorsEvent
                            {
                                EventID = item,
                                VendorsID = vendor.ID
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
                    vendor.VendorsEvents = events.Select(x => new VendorsEvent { EventID = x }).ToList();
                Db.Vendors.Add(vendor);
                await Db.SaveChangesAsync();
            }
            return true;
        }
        
        public async Task<List<Vendor>> GetVendorForSpecificEvent(int EventId)
        {
            bool isVendorNameSort = Db.Events.FirstOrDefault(x => x.ID == EventId).EnableVendorNameSort;
            bool isVendorNameAscendingSort = Db.Events.FirstOrDefault(x => x.ID == EventId).VendorSortBy;

            bool isVendorTypeSort = Db.Events.FirstOrDefault(x => x.ID == EventId).EnableVendorTypeSort;
            bool isVendorTypeAscendingSort = Db.Events.FirstOrDefault(x => x.ID == EventId).VendorTypeSortOrder;

            if (isVendorNameSort && isVendorTypeSort)
            {
                if (isVendorNameAscendingSort && isVendorTypeAscendingSort)
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => Helpers.HtmlToString(z.Name)).ThenBy(a => a.VendorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else if (isVendorNameAscendingSort && !isVendorTypeAscendingSort)
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => Helpers.HtmlToString(z.Name)).ThenByDescending(a => a.VendorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else if (!isVendorNameAscendingSort && isVendorTypeAscendingSort)
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => Helpers.HtmlToString(z.Name)).ThenBy(a => a.VendorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => Helpers.HtmlToString(z.Name)).ThenByDescending(a => a.VendorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
            }
            else if (isVendorNameSort)
            {
                if (isVendorNameAscendingSort)
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => Helpers.HtmlToString(z.Name)).ToList();
                }
                else
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => Helpers.HtmlToString(z.Name)).ToList();
                }
            }
            else if (isVendorTypeSort)
            {
                if (isVendorTypeAscendingSort)
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderBy(z => z.VendorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
                else
                {
                    return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList().OrderByDescending(z => z.VendorsEvents.FirstOrDefault(b => b.EventID == EventId)?.UserPackageType?.Packageorder).ToList();
                }
            }
            else
            {
                return Db.Vendors.Where(x => x.VendorsEvents.Any(y => y.EventID == EventId)).ToList();
            }
        }
    }
}
