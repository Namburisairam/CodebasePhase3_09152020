using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.DataAccess.DataBase.Model;
using EventManagement.BusinessLogic.Business;
using TransportERP.Base;
using EventManagement.BusinessLogic.Models;
using System.Data.Entity;
using EventManagement.BusinessLogic.Classes;

namespace EventManagement.Controllers
{

    public class ReportsController : BaseController
    {
        Entities db = new Entities();

        CommonLogic commonLogic = new CommonLogic();

        // GET: Reports
        public async Task<ActionResult> Index(string SenderName = null, string AdminName = null, DateTime? Startdate = null, DateTime? Enddate = null, string SelectLevel = null)
        {
            try
            {
                AttendesLogic attendesLogic = new AttendesLogic();
                IEnumerable<SelectListItem> AdminNames = db.Attendes.Where(x => x.IsAdmin == true).Select(n => new SelectListItem { Text = n.FirstName, Value = n.FirstName }).ToList();
                //(await attendesLogic.GetAttendes(checkAttendeeValidity: new AtteendeeValidator())).Select(n => new SelectListItem { Text = n.FirstName, Value = n.FirstName }).OrderBy(s => s.Text).ToList();

                IEnumerable<EventSurveyData> eventdata = db.Events.Select(x => new EventSurveyData { EventID = x.ID, EventName = x.EventName }).ToList();
                ViewBag.Events = eventdata;

                ViewBag.AdminNames = AdminNames;

                if (AdminName != null && Startdate == null && Enddate == null)
                {
                    ViewBag.TextChat = db.TextChats.Where(s => (s.Attende.FirstName == AdminName || s.Attende1.FirstName == AdminName) && s.IsGroup == true).ToList();
                }
                else if (AdminName == null && Startdate != null && Enddate != null)
                {
                    ViewBag.TextChat = db.TextChats.Where(s => (s.Createdate >= Startdate) && (s.Createdate <= Enddate) && s.IsGroup == true).ToList();
                }
                else if (AdminName != null && Startdate != null && Enddate != null)
                {
                    ViewBag.TextChat = db.TextChats.Where(s => (s.Attende.FirstName == AdminName || s.Attende1.FirstName == AdminName) && (s.Createdate >= Startdate) && (s.Createdate <= Enddate) && s.IsGroup == true).ToList();
                }
                else
                {
                    ViewBag.TextChat = db.TextChats.Where(s => s.IsGroup == true).ToList();
                }

                IEnumerable<SelectListItem> AttendeesNames = db.Attendes.Select(n => new SelectListItem { Text = n.FirstName, Value = n.FirstName }).OrderBy(s => s.Text).ToList();
                ViewBag.AttendeeList = AttendeesNames;

                if (SenderName != null && Startdate == null && Enddate == null)
                {
                    ViewBag.UserMessages = db.TextChats.Where(s => s.Attende1.FirstName == SenderName || s.Attende.FirstName == SenderName).ToList();
                }
                else if (SenderName == null && Startdate != null && Enddate != null)
                {
                    ViewBag.UserMessages = db.TextChats.Where(s => (s.Createdate >= Startdate) && (s.Createdate <= Enddate)).ToList();
                }
                else if (SenderName != null && Startdate != null && Enddate != null)
                {
                    ViewBag.UserMessages = db.TextChats.Where(s => (s.Attende1.FirstName == SenderName || s.Attende.FirstName == SenderName) && (s.Createdate >= Startdate) && (s.Createdate <= Enddate)).ToList();
                }
                else
                {
                    ViewBag.UserMessages = new List<TextChat>();
                }

                ViewBag.AppUsage = db.UserSessions
                                    .Where(m => m.Platform == "Android" || m.Platform == "IOS")
                                    .GroupBy(n => n.Platform)
                                    .Select(n => new AppUsage
                                    {
                                        Platform = n.Key,
                                        count = n.Select(x => x.DeviceToken)
                                    .Distinct().Count()
                                    }).ToList();

                ViewBag.AppDownloads = commonLogic.GetAppDownloadsReport();

                ViewBag.TopSavedActivities = db.BookMarks
                                                .GroupBy(s => s.Activite.Name)
                                                .Select(n => new TopSavedActivities
                                                {
                                                    Name = n.Key,
                                                    count = n.Select(x => x.Activite.Name).Count()
                                                })
                                                .OrderByDescending(o => o.count)
                                                .Take(10)
                                                .ToList();


                ViewBag.TopVisitedPages = db.Sys_Log.GroupBy(s => s.Page)
                                           .Select(n => new TopVisitedPages
                                           {
                                               Page = n.Key,
                                               count = n.Select(x => x.Page).Count()
                                           })
                                           .OrderByDescending(o => o.count)
                                           .Take(10)
                                           .ToList();

                ViewBag.DownloadPhotos = db.Sys_Log.Where(s => s.Page == "Download picture")
                                           .GroupBy(s => s.Attende.FirstName)
                                           .Select(n => new DownloadPhotos
                                           {
                                               Name = n.Key,
                                               count = n.Select(x => x.ModifiedBy).Count()
                                           })
                                           .OrderByDescending(o => o.count)
                                           .Take(10)
                                           .ToList();

                if (SelectLevel == "Event")
                {
                    ViewBag.QuestionResponses = db.Responses.
                                           Join(db.Questions, response => response.QuestionID, question => question.ID, (response, question) => new { response, question }).
                                           Where(x => x.question.Survey.SurveyLevel == "Event Level" &&
                                                      x.question.ResponseType.ResponseName != "Text").
                                           GroupBy(s => new { s.response.QuestionID, s.response.OptionsSelected }).
                                           Select(n => new SurveyReport
                                           {
                                               Question = db.Questions.FirstOrDefault(x => x.ID == n.Key.QuestionID).QuestionText,
                                               Responses = db.Options.FirstOrDefault(x => x.ID.ToString() == n.Key.OptionsSelected).option1,
                                               Count = n.Select(x => x.response.OptionsSelected).Count()
                                           })
                                           .OrderByDescending(o => o.Count)
                                           .ToList();
                    ViewBag.Level = "Event";
                }
                else if (SelectLevel == "App")
                {
                    ViewBag.QuestionResponses = db.Responses.ToList().
                        Join(db.Questions, response => response.QuestionID, question => question.ID, (response, question) => new { response, question }).
                        Where(x => x.question.Survey.SurveyLevel == "App Level" && x.question.ResponseType.ResponseName == "Rating").
                        GroupBy(s => new { s.response.QuestionID }).
                        Select(n => new SurveyReport
                        {
                            Question = db.Questions.FirstOrDefault(x => x.ID == n.Key.QuestionID).QuestionText,
                            Average = Math.Round(n.Select(x => Convert.ToInt32(x.response.OptionsSelected)).ToList().Average(), 2)
                        }).ToList();
                    ViewBag.Level = "App";
                }
                else if (SelectLevel == "Activity")
                {
                    ViewBag.QuestionResponses = db.Responses.
                                               Join(db.Questions, response => response.QuestionID, question => question.ID, (response, question) => new { question, response }).
                                               Where(x => x.question.Survey.SurveyLevel == "Activity Level").
                                               GroupBy(s => new { s.response.QuestionID, s.response.OptionsSelected }).
                                               Select(n => new SurveyReport
                                               {
                                                   Question = db.Questions.FirstOrDefault(x => x.ID == n.Key.QuestionID).QuestionText,
                                                   Responses = db.Options.FirstOrDefault(x => x.ID.ToString() == n.Key.OptionsSelected).option1,
                                                   Count = n.Select(x => x.response.OptionsSelected).Count()
                                               })
                                               .OrderByDescending(o => o.Count)
                                               .ToList();
                    ViewBag.Level = "Activity";
                }
                else
                {
                    ViewBag.QuestionResponses = new List<SurveyReport>();
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetMessagesbyUser(string SenderName, DateTime? date1 = null, DateTime? date2 = null)
        {
            return new RedirectResult(Url.Action("Index", new { SenderName = SenderName, Startdate = date1, Enddate = date2 }) + "#ParticipantsMessages");
        }
        public async Task<ActionResult> GetSurveyReports(string selectLevel)
        {
            return new RedirectResult(Url.Action("Index", new { SelectLevel = selectLevel }) + "#SurveyReports");
        }

    }
}

