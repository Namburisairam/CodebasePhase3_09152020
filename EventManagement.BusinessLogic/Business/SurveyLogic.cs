using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.BusinessLogic.Models;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class SurveyLogic : BaseLogic
    {
        public async Task<List<TemplateSurvey>> GetTemplates(int? templateID)
        {
            return await Db.TemplateSurveys.ToListAsync();
        }
        
        public async Task<List<TemplateSurvey>> GetAllTemplates()
        {
            return await Db.TemplateSurveys.Select(x => new TemplateSurvey { TSurveyID = x.TSurveyID, TSurveyName = x.TSurveyName }).ToListAsync();
        }

        public async Task<List<SurveyVendor>> GetVendors()
        {
            return await Db.Vendors.Select(x => new SurveyVendor { vendorID = x.ID, vendorName = x.Name }).ToListAsync();
        }

        public async Task<List<SurveySponsor>> GetSponsors()
        {
            return await Db.Sponsors.Select(x => new SurveySponsor { sponsorId = x.ID, sponsorName = x.Name }).ToListAsync();
        }

        public async Task<List<ResponseType>> GetResponseTypes()
        {
            return await Db.ResponseTypes.ToListAsync();
        }
        
        public async Task<List<EventSurveyData>> GetAllEvents()
        {
            return await Db.Events.Select(x => new EventSurveyData { EventID = x.ID, EventName = x.EventName }).ToListAsync();
        }
        
        public async Task<List<SurveyEventActivities>> GetEventActivities(int? EventID)
        {
            return await Db.Activites.Where(x => x.EventID == EventID).Select(y => new SurveyEventActivities { ActivityId = y.ID, ActivityName = y.Name }).ToListAsync();
        }

        public async Task<List<SurveyVendor>> GetEventVendors(int EventId)
        {
            return await Db.VendorsEvents.Where(x=> x.EventID == EventId).Select(y => new SurveyVendor{ vendorID = y.Vendor.ID , vendorName= y.Vendor.Name }).ToListAsync();
        }

        public async Task<List<SurveySponsor>> GetEventSponsors(int EventId)
        {
            return await Db.SponsorsEvents.Where(x => x.EventID == EventId).Select(y => new SurveySponsor { sponsorId = y.Sponsor.ID, sponsorName = y.Sponsor.Name}).ToListAsync();
        }


        public async Task<List<SurveyEventActivities>> GetActivities()
        {
            return await Db.Activites.Select(x => new SurveyEventActivities { ActivityId = x.ID, ActivityName = x.Name }).ToListAsync();
        }
        
        public async Task<List<SurveyList>> GetAllSurveys()
        {
            return await Db.Surveys.Select(x => new SurveyList { SurveyID = x.SurveyID, SurveyName = x.SurveyName }).ToListAsync();
        }
        
        public async Task<List<SurveyModel>> GetSurveyData(int surveyID)
        {
            try
            {
                List<SurveyModel> surveydata = await Db.Surveys.Where(x => x.SurveyID == surveyID)
                          .Select(y => new SurveyModel
                          {
                              SurveyID = y.SurveyID,
                              SurveyName = y.SurveyName,
                              SurveyDescription = y.SurveyDescription,
                              SurveyLevel = y.SurveyLevel,
                              EventID = y.EventID,
                              ActivityID = y.ActivityID,
                              TemplateID = y.SurveyTemplateID,
                              VendorID = y.VendorID,
                              SponsorID = y.SponsorID,
                              QuestionsOptions = y.Questions.Where(a => a.SurveyID == surveyID)
                                                   .Select(b => new SurveyQuestions
                                                   {
                                                       QID = b.ID,
                                                       QuestionText = b.QuestionText,
                                                       QuestionResponseType = b.QuestionResponseType,
                                                       options = b.Options.Select(c => new OptionData { OptionID = c.ID, OptionText = c.option1 }).ToList()
                                                   }).ToList()
                          }).ToListAsync();
                return surveydata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public async Task<List<TemplateModel>> GetTemplateData(int templateID)
        {
            List<TemplateModel> templatedata = await Db.TemplateSurveys.Where(x => x.TSurveyID == templateID)
                      .Select(y => new TemplateModel
                      {
                          TemplateName = y.TSurveyName,
                          TemplateDescription = y.TSurveyDescription,
                          TemplateLevel = y.TemplateLevel,
                          QuestionsOptions = y.TemplateQuestions.Where(a => a.TQSurveyID == templateID)
                                             .Select(b => new TemplateQuestions
                                             {
                                                 TQID = b.TQuestionID,
                                                 TQuestionText = b.TQuestionText,
                                                 TQuestionResponseType = b.TQuestionResponseType,
                                                 TQoptions = b.TemplateQuestionOptions.Select(c => new OptionData { OptionID = c.TQOptionID, OptionText = c.TQOptionText }).ToList()
                                             }).ToList()
                      }).ToListAsync();
            return templatedata;
        }
        
        public void AddNewQuestion(int? templateQuestionID, int TSurveyID, string TQuestionText, int QuestionResponseType, List<string> Options)
        {

            if (templateQuestionID != null)
            {
                var Questions = Db.TemplateQuestions.FirstOrDefault(x => x.TQuestionID == templateQuestionID && x.TQSurveyID == TSurveyID);
                Questions.TQuestionText = TQuestionText;
                Questions.TQuestionResponseType = QuestionResponseType;
                Questions.TQSurveyID = TSurveyID;
                List<TemplateQuestionOption> questionOptions = Db.TemplateQuestionOptions.Where(x => x.TQuestionID == templateQuestionID).ToList();
                foreach (var item in questionOptions)
                {
                    Db.TemplateQuestionOptions.Remove(item);
                }
                foreach (var option in Options)
                {
                    Db.TemplateQuestionOptions.Add(new TemplateQuestionOption
                    {
                        TQuestionID = templateQuestionID,
                        TQOptionText = option
                    });
                    //Db.SaveChanges();
                }
                Db.SaveChanges();
            }
            else
            {
                if (QuestionResponseType != 3 && QuestionResponseType != 5)
                {
                    List<TemplateQuestionOption> templateQuestionOptions = new List<TemplateQuestionOption>();
                    foreach (var item in Options)
                    {
                        templateQuestionOptions.Add(new TemplateQuestionOption { TQOptionText = item });
                    }
                    Db.TemplateQuestions.Add(new TemplateQuestion
                    {
                        TQSurveyID = TSurveyID,
                        TQuestionText = TQuestionText,
                        TQuestionResponseType = QuestionResponseType,
                        TemplateQuestionOptions = templateQuestionOptions
                    });
                    Db.SaveChanges();
                }
                else
                {
                    Db.TemplateQuestions.Add(new TemplateQuestion
                    {
                        TQSurveyID = TSurveyID,
                        TQuestionText = TQuestionText,
                        TQuestionResponseType = QuestionResponseType
                    });
                    Db.SaveChanges();
                }
            }
        }
        
        public void AddQuestionsToSurvey(int? surveyQuestionID, int surveyID, string sQuestionText, int sResponseType, List<string> sOptions)
        {
            if (surveyQuestionID != null)
            {
                var questions = Db.Questions.FirstOrDefault(x => x.ID == surveyQuestionID && x.SurveyID == surveyID);
                questions.QuestionText = sQuestionText;
                questions.QuestionResponseType = sResponseType;
                questions.SurveyID = surveyID;
                List<Option> questionOptions = Db.Options.Where(x => x.QID == surveyQuestionID).ToList();
                foreach (var item in questionOptions)
                {
                    Db.Options.Remove(item);
                }
                Db.SaveChanges();
                foreach (var option in sOptions)
                {
                    Db.Options.Add(new Option
                    {
                        QID = surveyQuestionID ?? 0,
                        option1 = option
                    });
                    Db.SaveChanges();
                }

            }
            else
            {
                if (sResponseType != 3 && sResponseType != 5)
                {
                    List<Option> opt = new List<Option>();
                    foreach (var item in sOptions)
                    {
                        opt.Add(new Option { option1 = item });
                    }
                    Db.Questions.Add(new Question
                    {
                        SurveyID = surveyID,
                        QuestionText = sQuestionText,
                        QuestionResponseType = sResponseType,
                        CreateDate = DateTime.Now,
                        Options = opt
                    });
                    Db.SaveChanges();
                }
                else
                {
                    Db.Questions.Add(new Question
                    {
                        SurveyID = surveyID,
                        QuestionText = sQuestionText,
                        QuestionResponseType = sResponseType,
                        CreateDate = DateTime.Now
                    });
                    Db.SaveChanges();
                }
            }
        }
        
        public async Task<List<TQuestionswithOptions>> GetQuestionOptions(int? templateID)
        {
            if (templateID == null)
            {
                var questionwithoptions = await Db.TemplateQuestions.Select(x => new TQuestionswithOptions
                {
                    QID = x.TQuestionID,
                    QuestionText = x.TQuestionText,
                    QuestionResponseType = x.TQuestionResponseType,
                    options = x.TemplateQuestionOptions.Select(y => new OptionData { OptionID = y.TQOptionID, OptionText = y.TQOptionText }).ToList()
                    //options = x.TemplateQuestionOptions.Select(y => y.TQOptionText ).ToList()
                }).ToListAsync();
                return questionwithoptions;
            }
            else
            {
                var questionwithoptions = await Db.TemplateQuestions.Where(x => x.TemplateSurvey.TSurveyID == templateID).Select(x => new TQuestionswithOptions
                {
                    QID = x.TQuestionID,
                    QuestionText = x.TQuestionText,
                    QuestionResponseType = x.TQuestionResponseType,
                    options = x.TemplateQuestionOptions.Select(y => new OptionData { OptionID = y.TQOptionID, OptionText = y.TQOptionText }).ToList()
                }).ToListAsync();
                return questionwithoptions;
            }

        }
        
        public async Task<List<SurveyQuestions>> GetSurveyQuestionOptions(int? surveyID)
        {
            if (surveyID == null)
            {
                var surveyQuestionOptions = await Db.Questions.Select(y => new SurveyQuestions
                {
                    QID = y.ID,
                    QuestionText = y.QuestionText,
                    QuestionResponseType = y.QuestionResponseType,
                    options = y.Options.Select(a => new OptionData { OptionID = a.ID, OptionText = a.option1 }).ToList()
                }).ToListAsync();
                return surveyQuestionOptions;
            }
            else
            {
                var surveyQuestionOptions = await Db.Questions.Where(x => x.Survey.SurveyID == surveyID).Select(y => new SurveyQuestions
                {
                    QID = y.ID,
                    QuestionText = y.QuestionText,
                    QuestionResponseType = y.QuestionResponseType,
                    options = y.Options.Select(a => new OptionData { OptionID = a.ID, OptionText = a.option1 }).ToList()
                }).ToListAsync();
                return surveyQuestionOptions;
            }
        }
        
        public bool DeleteTemplateQuestion(int id)
        {
            try
            {
                List<TemplateQuestionOption> questionOptions = Db.TemplateQuestionOptions.Where(x => x.TQuestionID == id).ToList();
                foreach (var item in questionOptions)
                {
                    Db.TemplateQuestionOptions.Remove(item);
                }
                var question = Db.TemplateQuestions.Find(id);
                Db.TemplateQuestions.Remove(question);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public bool DeleteSurveyQuestions(int id)
        {
            try
            {
                List<Option> surveyquestionOptions = Db.Options.Where(x => x.QID == id).ToList();
                foreach (var item in surveyquestionOptions)
                {
                    Db.Options.Remove(item);
                }
                var question = Db.Questions.Find(id);
                Db.Questions.Remove(question);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public bool SaveTemplate(int? templateid, string templateName, string templateDescription, string templateLevel)
        {
            if (templateid == null)
            {
                Db.TemplateSurveys.Add(new TemplateSurvey
                {
                    TSurveyName = templateName,
                    TSurveyDescription = templateDescription,
                    TemplateLevel = templateLevel
                });
                Db.SaveChanges();
                return true;
            }
            else
            {
                var template = Db.TemplateSurveys.Find(templateid);
                template.TSurveyDescription = templateDescription;
                template.TSurveyName = templateName;
                Db.SaveChanges();
                return true;
            }

        }
        
        public void ReplicateTemplate(int id)
        {
            Db.DuplicateSurveyTemplate(id);
        }
        
        public void SaveSurvey(int? surveyID, string surveyName, string surveyLevel, string surveyDescription, int? surveyEventID, int? surveyEventActivityID, int? surveyTemplate, int? surveyVendorID, int? surveySponsorID)
        {
            var surveyData = Db.Surveys.FirstOrDefault(x => x.SurveyID == surveyID);

            if (surveyData != null)
            {
                surveyData.SurveyName = surveyName;
                surveyData.SurveyDescription = surveyDescription;
            }
            else
            {
                Db.Surveys.Add(new Survey
                {
                    SurveyName = surveyName,
                    SurveyLevel = surveyLevel,
                    SurveyDescription = surveyDescription,
                    EventID = surveyEventID,
                    ActivityID = surveyEventActivityID,
                    SurveyTemplateID = surveyTemplate,
                    VendorID = surveyVendorID,
                    SponsorID = surveySponsorID
                });
            }
            Db.SaveChanges();
        }
        
        public void SaveSurveyQuestions(string surveyName, int? surveyTemplate)
        {
            List<TemplateQuestion> questions = Db.TemplateQuestions.Where(x => x.TQSurveyID == surveyTemplate).ToList();
            List<TemplateQuestionOption> options = new List<TemplateQuestionOption>();
            var surveyID = Db.Surveys.FirstOrDefault(x => x.SurveyName == surveyName && x.SurveyTemplateID == surveyTemplate).SurveyID;

            foreach (var question in questions)
            {
                Db.Questions.Add(new Question
                {
                    QuestionText = question.TQuestionText,
                    QuestionResponseType = question.TQuestionResponseType,
                    SurveyID = Convert.ToInt32(surveyID),
                    CreateDate = DateTime.Now
                });
                Db.SaveChanges();
                var TQID = Db.Questions.FirstOrDefault(x => x.QuestionText == question.TQuestionText && x.SurveyID == surveyID).ID;
                options = Db.TemplateQuestionOptions.Where(x => x.TQuestionID == question.TQuestionID).ToList();
                foreach (var option in options)
                {
                    Db.Options.Add(new Option
                    {
                        QID = Convert.ToInt32(TQID),
                        option1 = option.TQOptionText
                    });
                    Db.SaveChanges();
                }
            }
        }
        
        public void DeleteTemplate(int templateID)
        {
            try
            {
                var template = Db.TemplateSurveys.Find(templateID);
                if (template != null)
                {
                    List<TemplateQuestion> tquestions = Db.TemplateQuestions.Where(x => x.TQSurveyID == templateID).ToList();
                    foreach (var question in tquestions)
                    {
                        List<TemplateQuestionOption> questionOptions = Db.TemplateQuestionOptions.Where(x => x.TQuestionID == question.TQuestionID).ToList();
                        foreach (var item in questionOptions)
                        {
                            Db.TemplateQuestionOptions.Remove(item);
                        }
                        Db.TemplateQuestions.Remove(question);
                        Db.SaveChanges();
                    }
                    List<Survey> surveys = Db.Surveys.Where(x => x.SurveyTemplateID == templateID).ToList();
                    foreach (var survey in surveys)
                    {
                        survey.SurveyTemplateID = null;
                    }
                    Db.TemplateSurveys.Remove(template);
                    Db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        
        public void DeleteSurvey(int surveyID)
        {
            var survey = Db.Surveys.Find(surveyID);
            List<Question> squestions = Db.Questions.Where(x => x.SurveyID == surveyID).ToList();
            foreach (var question in squestions)
            {
                List<Option> questionOptions = Db.Options.Where(x => x.QID == question.ID).ToList();
                foreach (var item in questionOptions)
                {
                    Db.Options.Remove(item);
                }
                Db.Questions.Remove(question);
                Db.SaveChanges();
            }
            Db.Surveys.Remove(survey);
            Db.SaveChanges();
        }
        
        public void DeleteTemplateOptions(int templateOptionID, int templateQuestionID)
        {
            var QuestionOption = Db.TemplateQuestionOptions.FirstOrDefault(x => x.TQuestionID == templateQuestionID && x.TQOptionID == templateOptionID);
            Db.TemplateQuestionOptions.Remove(QuestionOption);
            Db.SaveChanges();
        }
        
        public void DeleteSurveyOptions(int surveyOptionID, int surveyQuestionID)
        {
            var QuestionOption = Db.Options.FirstOrDefault(x => x.QID == surveyQuestionID && x.ID == surveyOptionID);
            Db.Options.Remove(QuestionOption);
            Db.SaveChanges();
        }
    }
}
