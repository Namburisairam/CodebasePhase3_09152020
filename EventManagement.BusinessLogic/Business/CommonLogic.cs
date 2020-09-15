using System;
using System.Collections.Generic;
using System.Linq;
using EventManagement.BusinessLogic.BussinessBase;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Business
{
    public class CommonLogic : BaseLogic
    {
        public void SaveLog(string screen, string operationType, int createdBy, string message)
        {
            DateTime dateTime = DateTime.Now.ToUniversalTime();

            Db.ApplicationLogs.Add(new ApplicationLog
            {
                Screen = screen,
                OperationType = operationType,
                CreatedBy = createdBy,
                CreatedDate = dateTime,
                Message = message
            });


            Db.SaveChanges();
        }

        public void ApplicationCloseTime(int attendeeId)
        {
            ApplicationAccessInfo applicationAccessInfo = Db.ApplicationAccessInfoes.Where(s => s.AttendeeId == attendeeId).OrderByDescending(s => s.ApplicationStartTime).Take(1).FirstOrDefault();

            if (applicationAccessInfo != null)
            {
                DateTime newEndTime = DateTime.Now.ToUniversalTime();

                if (!applicationAccessInfo.ApplicationEndTime.HasValue)
                {
                    applicationAccessInfo.ApplicationEndTime = newEndTime;
                    Db.SaveChanges();
                }
            }
        }

        public void ApplicationStartTime(int attendeeId)
        {
            ApplicationAccessInfo applicationAccessInfo = Db.ApplicationAccessInfoes.Where(s => s.AttendeeId == attendeeId).OrderByDescending(s => s.ApplicationStartTime).Take(1).FirstOrDefault();

            DateTime newStartTime = DateTime.Now.ToUniversalTime();

            if (applicationAccessInfo == null)
            {
                ApplicationAccessInfo applicationAccessInfoNew = new ApplicationAccessInfo
                {
                    AttendeeId = attendeeId,
                    ApplicationStartTime = newStartTime
                };
                Db.ApplicationAccessInfoes.Add(applicationAccessInfoNew);

                AppDownload appDownload = new AppDownload();
                appDownload.AttendeeId = attendeeId;
                appDownload.DownloadDate = newStartTime;
                appDownload.UserName = GetAttendeeFullName(attendeeId);

                Db.AppDownloads.Add(appDownload);
            }
            else
            {
                if (!applicationAccessInfo.ApplicationEndTime.HasValue)
                {
                    DateTime newEndDate = applicationAccessInfo.ApplicationStartTime.Value.Add(TimeSpan.FromMinutes(45));

                    while (newEndDate > newStartTime)
                    {
                        newEndDate = newEndDate.Add(new TimeSpan(0, -5, 0));
                    }

                    applicationAccessInfo.ApplicationEndTime = newEndDate;
                    Db.SaveChanges();
                }

                ApplicationAccessInfo applicationAccessInfoNew = new ApplicationAccessInfo();
                applicationAccessInfoNew.AttendeeId = attendeeId;
                applicationAccessInfoNew.ApplicationStartTime = newStartTime;
                Db.ApplicationAccessInfoes.Add(applicationAccessInfoNew);
            }

            Db.SaveChanges();
        }

        public List<AppDownloadsReport> GetAppDownloadsReport()
        {
            List<AppDownload> appDownloads = Db.AppDownloads.ToList();

            List<AppDownloadsReport> appDownloadsReports = new List<AppDownloadsReport>();

            foreach (AppDownload appDownload in appDownloads)
            {
                AppDownloadsReport appDownloadsReport = new AppDownloadsReport
                {
                    ID = appDownload.ID,
                    UserName = appDownload.UserName,
                    DownloadDate = appDownload.DownloadDate.ToLocalTime(),
                    LastVisitDate = GetLastVisitDate(appDownload.AttendeeId.Value),
                    LastLoginDate = GetLastLoginDate(appDownload.AttendeeId.Value)
                };

                appDownloadsReports.Add(appDownloadsReport);
            }

            return appDownloadsReports;
        }

        private DateTime GetLastVisitDate(int attendeeId)
        {
            ApplicationAccessInfo applicationAccessInfo = Db.ApplicationAccessInfoes.Where(s => s.AttendeeId == attendeeId).OrderByDescending(s => s.ApplicationStartTime).Take(1).FirstOrDefault();


            if (applicationAccessInfo == null)
            {
                return DateTime.Now;
            }
            else
            {
                if (applicationAccessInfo.ApplicationEndTime.HasValue)
                {
                    return applicationAccessInfo.ApplicationEndTime.Value.ToLocalTime();
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }


        private DateTime GetLastLoginDate(int attendeeId)
        {
            ApplicationAccessInfo applicationAccessInfo = Db.ApplicationAccessInfoes.Where(s => s.AttendeeId == attendeeId).OrderByDescending(s => s.ApplicationStartTime).Take(1).FirstOrDefault();


            if (applicationAccessInfo == null)
            {
                return DateTime.Now;
            }
            else
            {
                if (applicationAccessInfo.ApplicationStartTime.HasValue)
                {
                    return applicationAccessInfo.ApplicationStartTime.Value.ToLocalTime();
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }


        private string GetAttendeeFullName(int attendeeId)
        {
            Attende attende = Db.Attendes.FirstOrDefault(s => s.ID == attendeeId);
            return string.Concat(attende.FirstName, ' ', attende.Lastname);
        }


        public bool IsValidAppVersion(string appVersion, string platform)
        {
            string dbAppVersion = string.Empty;

            if (string.Equals(platform, "iOS", StringComparison.CurrentCultureIgnoreCase))
            {
                dbAppVersion = Db.SysSettings.FirstOrDefault(s => s.SysSettingKey == "MobileAppVersion-iOS").SysSettingValue;
            }
            else
            {
                dbAppVersion = Db.SysSettings.FirstOrDefault(s => s.SysSettingKey == "MobileAppVersion-Android").SysSettingValue;
            }

            return string.Equals(appVersion, dbAppVersion, StringComparison.CurrentCultureIgnoreCase);
        }

        public void InsertNotificationId(string authToken, string connectionId)
        {
            UserSession userSession = Db.UserSessions.FirstOrDefault(x => x.AuthToken == authToken);

            if (userSession != null)
            {
                userSession.DeviceToken = connectionId;
            }

            Db.SaveChanges();
        }


    }
}
