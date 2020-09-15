#region Libraries
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Classes;
using EventManagement.BusinessLogic.Interfaces;
using EventManagement.DataAccess.DataBase.Model;
#endregion

namespace EventManagement.BusinessLogic.Business
{
    public class AttendesLogic : BaseLogic
    {
        public async Task<List<Attende>> GetAttendes(string value = "", IEntityValidity<Attende> checkAttendeeValidity = null)
        {
            checkAttendeeValidity = checkAttendeeValidity ?? new AtteendeeValidator();
            if (string.IsNullOrEmpty(value))
                return await Db.Attendes.Where(checkAttendeeValidity.CheckValidity).ToListAsync();
            else return await Db.Attendes.Where(x => x.FirstName.StartsWith(value) || x.Email.Contains(value)).ToListAsync();
        }

        public async Task<bool> BookMarkActivity(int attendesID, int activityId)
        {
            if (Db.BookMarks.Any(x => x.AttendesID == attendesID && activityId == x.ActivityID))
            {
                var bookmark = Db.BookMarks.FirstOrDefault(x => x.AttendesID == attendesID && x.ActivityID == activityId);
                Db.BookMarks.Remove(bookmark);
                await Db.SaveChangesAsync();
            }
            else
            {
                try
                {
                    BookMark bm = new BookMark
                    {
                        ActivityID = activityId,
                        AttendesID = attendesID
                    };
                    Db.BookMarks.Add(bm);
                    //Db.BookMarks.Add(new BookMark
                    //{
                    //    AttendesID = attendesID,
                    //    ActivityID = activityId,
                    //});
                    await Db.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                }

            }
            return true;
        }

        public async Task<Attende> GetAttendesById(int id)
        {
            return await Db.Attendes.FindAsync(id);
        }

        public void UpdateUserProfile(Attende attende, int attendesID, string ImageSavePath)
        {
            var dbAttendee = Db.Attendes.Find(attendesID);
            dbAttendee.FirstName = attende.FirstName;
            dbAttendee.Description = attende.Description;
            dbAttendee.FacebookURL = attende.FacebookURL;
            dbAttendee.TwitterURL = attende.TwitterURL;
            dbAttendee.InstagramURL = attende.InstagramURL;
            dbAttendee.LinkedinURL = attende.LinkedinURL;
            dbAttendee.Lastname = attende.Lastname;
            dbAttendee.CompanyName = attende.CompanyName;
            dbAttendee.TitleName = attende.TitleName;
            dbAttendee.Website = attende.Website;
            if (dbAttendee.IsUploadedImage == false)
            {
                dbAttendee.Thumbnail = GenerateImageFromName(attende.FirstName, attende.Lastname, ImageSavePath);
            }
            Db.SaveChanges();
        }

        public async Task<bool> DeletePicture(int id, string imageSavePath)
        {
            if (id > 0)
            {
                var attendee = await Db.Attendes.FindAsync(id);
                attendee.Thumbnail = GenerateImageFromName(attendee.FirstName, attendee.Lastname, imageSavePath);
                attendee.IsUploadedImage = false;
                await Db.SaveChangesAsync();
                return true;

            }
            else
            {
                return false;
            }

        }

        public async Task<string> UniqueAttendeeID(int id)
        {
            var a = await Db.Attendes.FindAsync(id);
            return a.Unique_Participant_ID;
        }

        public async Task<object> GetAttendeEvents(int attendeeId)
        {
            return await Db.AttendesEvents.Where(x => x.AttendesID == attendeeId).Select(x => new
            {
                x.Event.EventName,
                x.EventID,
            }).ToListAsync();
        }

        public async Task<object> GetBookMarksByAttendeId(int attendId)
        {
            var bookmarks = await Db.BookMarks.Where(x => x.AttendesID == attendId).ToListAsync();
            return new
            {
                Data = bookmarks.Select(x => new
                {
                    x.Activite.Description,
                    x.Activite.Address,
                    x.Activite.StartTime,
                    x.Activite.EndTime,
                    x.Activite.Thumbnail,
                    x.Activite.Status,
                })
            };
        }

        public async Task<bool> DeleteAttende(int id)
        {
            // delete code here
            return false;
        }

        public async Task<bool> AddUpdateAttendee(Attende attende, List<int> events, bool isUploadedImage)
        {
            if (attende.ID > 0)
            {
                var updateAttendee = Db.Attendes.Find(attende.ID);
                updateAttendee.FirstName = attende.FirstName;
                updateAttendee.Lastname = attende.Lastname;
                updateAttendee.InstagramURL = attende.InstagramURL;
                updateAttendee.Status = attende.Status;
                updateAttendee.TwitterURL = attende.TwitterURL;
                updateAttendee.FacebookURL = attende.FacebookURL;
                updateAttendee.Description = attende.Description;
                updateAttendee.LinkedinURL = attende.LinkedinURL;
                updateAttendee.CompanyName = attende.CompanyName;
                updateAttendee.TitleName = attende.TitleName;
                updateAttendee.Website = attende.Website;
                updateAttendee.Email = attende.Email;
                updateAttendee.IsUploadedImage = isUploadedImage;
                if (!string.IsNullOrEmpty(attende.Thumbnail))
                    updateAttendee.Thumbnail = attende.Thumbnail;
                if (events == null)
                {
                    var removeAll = Db.AttendesEvents.Where(x => x.AttendesID == attende.ID);
                    Db.AttendesEvents.RemoveRange(removeAll);
                }
                else
                {
                    var removeEvent = Db.AttendesEvents.Where(x => x.AttendesID == attende.ID && !events.Contains(x.EventID));
                    if (removeEvent.Count() > 0)
                    {
                        Db.AttendesEvents.RemoveRange(removeEvent);
                        await Db.SaveChangesAsync();
                    }
                    foreach (var item in events)
                    {
                        if (!Db.AttendesEvents.Any(x => x.EventID == item && x.AttendesID == attende.ID))
                        {
                            Db.AttendesEvents.Add(new AttendesEvent
                            {
                                EventID = item,
                                AttendesID = attende.ID
                            });
                        }
                    }
                }

                updateAttendee.EnableMessaging = attende.EnableMessaging;
                updateAttendee.EnableAlertEmails = attende.EnableAlertEmails ?? false;
                updateAttendee.EnableAttendeeMessagingEmails = attende.EnableAttendeeMessagingEmails ?? false;

                await Db.SaveChangesAsync();
                //update
            }
            else
            {
                //save
                if (events != null && events.Count > 0)
                    attende.AttendesEvents = events.Select(x => new AttendesEvent { EventID = x }).ToList();
                Db.Attendes.Add(attende);
                await Db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<Attende> GetAttendee(int? id)
        {
            if (id > 0)
                return await Db.Attendes.FindAsync(id);
            return new Attende() { EnableMessaging = true };
        }

        public async Task<int> ChangePhoto(int id, string filename)
        {
            try
            {
                var attendee = await Db.Attendes.FindAsync(id);
                attendee.Thumbnail = filename;
                attendee.Imageapprove = true;
                await Db.SaveChangesAsync();
                return attendee.ID;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<List<Attende>> GetAttendeeForSpecificEvent(int EventId, IEntityValidity<Attende> checkAttendeeValidity = null)
        {
            checkAttendeeValidity = checkAttendeeValidity ?? new AtteendeeValidator();
            return await Db.Attendes.Where(checkAttendeeValidity.CheckValidity).Where(x => x.AttendesEvents.Any(y => y.EventID == EventId)).ToListAsync();
        }

        public async Task<List<Attende>> GetAttendeeWithSpeakerForSpecificEvent(int EventId)
        {
            return await Db.Attendes.Where(x => x.AttendesEvents.Any(y => y.EventID == EventId) && !x.IsAdmin).ToListAsync();
        }


        public string GenerateImageFromName(string firstName, string lastName, string imageSavePath, bool draw_border = false)
        {
            var firstNameCharacter = !string.IsNullOrEmpty(firstName) ? firstName?.ToUpper()[0].ToString() : string.Empty;
            var lastNameCharacter = !string.IsNullOrEmpty(lastName) ? lastName?.ToUpper()[0].ToString() : string.Empty;

            string shortName = $"{firstNameCharacter}{lastNameCharacter}";
            return GenerateImage(shortName, imageSavePath, draw_border);
        }

        public async Task<string> GenerateAttendeeImageFromName(int id, string imageSavePath, bool draw_border = false)
        {
            var attendee = await Db.Attendes.FindAsync(id);
            attendee.Thumbnail = GenerateImageFromName(attendee.FirstName, attendee.Lastname, imageSavePath);
            await Db.SaveChangesAsync();
            return attendee.Thumbnail;
        }

        public async Task<bool> CheckIfEmailAlreadyExists(string email, int attendeeID)
        {
            var attendee = await GetAttendee(attendeeID);

            return attendee.Email != email && await Db.Attendes.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        }

        private string GenerateImage(string txt, string imageSavePath, bool draw_border = false)
        {
            const int width = 120;
            Color bg_color = Color.FromArgb(128, 128, 128);            
            Color fg_color = Color.White;
            Font fg_font = new Font(FontFamily.GenericSerif, 35);

            Bitmap bm = new Bitmap(width, width);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.TextRenderingHint = TextRenderingHint.AntiAlias;

                // Make the background transparent.
                gr.Clear(Color.Transparent);

                // Fill the background.
                Rectangle rect = new Rectangle(2, 2, width - 4, width - 4);
                //using (LinearGradientBrush bg_brush =
                //    new LinearGradientBrush(rect, Color.White,
                //    bg_color, LinearGradientMode.BackwardDiagonal))
                //{
                //    gr.FillEllipse(bg_brush, rect);
                //}

                using (Brush bg_brush =
                    new SolidBrush(bg_color))
                {
                    gr.FillEllipse(bg_brush, rect);
                }


                // Outline the background.
                if (draw_border)
                {
                    using (Pen bg_pen = new Pen(bg_color))
                    {
                        gr.DrawEllipse(bg_pen, rect);
                    }
                }

                // Draw the sample text.
                using (StringFormat string_format = new StringFormat())
                {
                    string_format.Alignment = StringAlignment.Center;
                    string_format.LineAlignment = StringAlignment.Center;
                    using (Brush fg_brush = new SolidBrush(fg_color))
                    {
                        gr.DrawString(txt, fg_font, fg_brush,
                            rect, string_format);
                    }
                }
            }
            var fileName = Guid.NewGuid().ToString() + ".jpg";
            bm.Save(Path.Combine(imageSavePath, fileName));
            return fileName;
        }

        public async Task<List<Attende>> GetPreferences(int AttendeeID)
        {
            return await Db.Attendes.Where(x => x.ID == AttendeeID).ToListAsync();
        }
        public async Task<bool> SavePreferences(int AttendeeID, bool EnableMessaging, bool EnableAlertEmails, bool EnableAttendeeMessagingEmails)
        {
            try
            {
                var attendees = Db.Attendes.Find(AttendeeID);
                attendees.EnableMessaging = EnableMessaging;
                attendees.EnableAlertEmails = EnableAlertEmails;
                attendees.EnableAttendeeMessagingEmails = EnableAttendeeMessagingEmails;
                await Db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool checkIfAttendeeMessagingEmailEnabled(int ID)
        {
            return Db.Attendes.FirstOrDefault(x => x.ID == ID)?.EnableAttendeeMessagingEmails ?? false;
        }

        public bool checkIfAttendeeAlertEmailEnabled(int ID)
        {
            return Db.Attendes.FirstOrDefault(x => x.ID == ID)?.EnableAlertEmails ?? false;
        }

        public bool checkIfAttendeeMessagingEnabled(int ID)
        {
            return Db.Attendes.FirstOrDefault(x => x.ID == ID)?.EnableMessaging ?? true;
        }

        public bool checkIfAttendeeMessagingEnabled(Attende attendee)
        {
            return attendee?.EnableMessaging ?? true;
        }

        public async Task<int> ChangeProfilePicture(int id, string ImageSavePath, string name)
        {
            var attendee = await Db.Attendes.FindAsync(id);
            AddNewImage(ImageSavePath, name);
            attendee.Thumbnail = name;
            await Db.SaveChangesAsync();
            return attendee.ID;
        }
    }
}





