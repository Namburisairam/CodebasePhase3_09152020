using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using EventManagement.BusinessLogic.Business;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;
using TransportERP.Base;

namespace EventManagement.Controllers
{
    public class SurveyController : BaseController
    {
        SurveyLogic surveyLogic = new SurveyLogic();
        CommonLogic commonLogic = new CommonLogic();

        // GET: Survey
        public async Task<ActionResult> Index(int? id, int? EventID, int? surveyID)
        {
            ViewBag.Templates = await surveyLogic.GetTemplates(id);
            ViewBag.ResponseTypes = await surveyLogic.GetResponseTypes();
            ViewBag.TemplateQuestionOptions = await surveyLogic.GetQuestionOptions(id);
            ViewBag.Events = await surveyLogic.GetAllEvents();
            ViewBag.Activities = await surveyLogic.GetEventActivities(id);
            ViewBag.SurveysList = await surveyLogic.GetAllSurveys();
            ViewBag.AllActivities = await surveyLogic.GetActivities();
            ViewBag.AllVendors = await surveyLogic.GetVendors();
            ViewBag.AllSponsors = await surveyLogic.GetSponsors();
            ViewBag.SurveyQuestionOptions = await surveyLogic.GetSurveyQuestionOptions(surveyID);
            return View();
        }
        
        public async Task<ActionResult> GetEventActivities(int EventID)
        {
            List<SurveyEventActivities> eventActivities = await surveyLogic.GetEventActivities(EventID);
            return Json(eventActivities, JsonRequestBehavior.AllowGet);
        }
        
        public async Task<ActionResult> GetEventVendors(int EventID)
        {
            List<SurveyVendor> eventVendors = await surveyLogic.GetEventVendors(EventID);
            return Json(eventVendors, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetEventSponsors(int EventID)
        {
            List<SurveySponsor> eventSponsors = await surveyLogic.GetEventSponsors(EventID);
            return Json(eventSponsors, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetActivities()
        {
            List<SurveyEventActivities> activities = await surveyLogic.GetActivities();
            return Json(activities, JsonRequestBehavior.AllowGet);
        }
        
        public async Task<ActionResult> GetSurveyData(int SurveyID)
        {
            List<SurveyModel> surveys = await surveyLogic.GetSurveyData(SurveyID);
            return Json(surveys, JsonRequestBehavior.AllowGet);
        }
        
        public async Task<ActionResult> GetTemplateData(int templateID)
        {
            List<TemplateModel> templates = await surveyLogic.GetTemplateData(templateID);
            return Json(templates, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult AddQuestion(int? TemplateQuestionID, int TSurveyID, string TQuestionText, int ResponseType, List<string> Options)
        {
            surveyLogic.AddNewQuestion(TemplateQuestionID, TSurveyID, TQuestionText, ResponseType, Options);
            return Redirect(Url.Action("Index") + "#Template");
        }
        
        public ActionResult AddSurveyQuestion(int? SurveyQuestionID, int S_QID, string SQuestionText, int SQuestionResponseType, List<string> SOptions)
        {
            surveyLogic.AddQuestionsToSurvey(SurveyQuestionID, S_QID, SQuestionText, SQuestionResponseType, SOptions);
            return Redirect(Url.Action("Index") + "#Survey");
        }
        
        [HttpPost]
        public ActionResult DeleteTemplateQuestion(int id)
        {
            surveyLogic.DeleteTemplateQuestion(id);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Template", "Delete", userId, $"The Template Question id: {id} is deleted by {userName}.");

            return Redirect(Url.Action("Index") + "#Template");
        }
        
        [HttpPost]
        public ActionResult DeleteSurveyQuestions(int id)
        {
            surveyLogic.DeleteSurveyQuestions(id);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Survey", "Delete", userId, $"The Survey Question id: {id} is deleted by {userName}.");

            return Redirect(Url.Action("Index") + "#Survey");
        }
        
        [HttpPost]
        public ActionResult SaveTemplate(int? Templateid, string TemplateName, string TemplateDescription, string TemplateLevel)
        {
            surveyLogic.SaveTemplate(Templateid, TemplateName, TemplateDescription, TemplateLevel);
            return Redirect(Url.Action("Index") + "#Template");
        }
        
        public ActionResult ReplicateTemplate(int id)
        {
            surveyLogic.ReplicateTemplate(id);
            return Redirect(Url.Action("Index") + "#Template");
        }
        
        public ActionResult SaveSurvey(int? SurveyID, string SurveyName, string SurveyLevel, string SurveyDescription, int? SurveyEventID, int? SurveyEventActivityID, int? SurveyTemplate, int? surveyVendorID, int? surveySponsorID)
        {
            surveyLogic.SaveSurvey(SurveyID, SurveyName, SurveyLevel, SurveyDescription, SurveyEventID, SurveyEventActivityID, SurveyTemplate,surveyVendorID,surveySponsorID);

            if (SurveyTemplate != null)
            {
                surveyLogic.SaveSurveyQuestions(SurveyName, SurveyTemplate);
            }
            return Redirect(Url.Action("Index") + "#Survey");

        }
        
        public async Task<ActionResult> DeleteTemplate(int id)
        {
            surveyLogic.DeleteTemplate(id);
            List<TemplateSurvey> templateSurveys = await surveyLogic.GetTemplates(null);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Template", "Delete", userId, $"The Template id: {id} is deleted by {userName}.");

            return Json(templateSurveys, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetAllTemplates()
        {
            List<TemplateSurvey> templateSurveys = await surveyLogic.GetAllTemplates();
            return Json(templateSurveys, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSurvey(int id)
        {
            surveyLogic.DeleteSurvey(id);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Survey", "Delete", userId, $"The survey id: {id} is deleted by {userName}.");

            return Redirect(Url.Action("Index") + "#Survey");
        }
        
        public ActionResult DeleteTemplateOptions(int TOID, int TQID)
        {
            surveyLogic.DeleteTemplateOptions(TOID, TQID);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Survey", "Delete", userId, $"The template option id: {TOID} for the template question id : {TQID} is deleted by {userName}.");

            return Redirect(Url.Action("Index") + "#Template");
        }
        
        public ActionResult DeleteSurveyOptions(int SOID, int SQID)
        {
            surveyLogic.DeleteSurveyOptions(SOID, SQID);

            int userId = CurrentUserSession.Attende.ID;
            string userName = string.Concat(CurrentUserSession.Attende.FirstName, " ", CurrentUserSession.Attende.Lastname);
            commonLogic.SaveLog("Survey", "Delete", userId, $"The Survey option id: {SOID} for the Survey question id : {SQID} is deleted by {userName}.");

            return Json(new { success = true });
        }
    }
}