using System;
using EventManagement.BusinessLogic.BussinessBase;

namespace EventManagement.BusinessLogic.Business
{
    public class Error : BaseLogic
    {
        public void LogErrorToDB(Exception exception, string area)
        {
            try
            {
                Db.Error_Logs.Add(new DataAccess.DataBase.Model.Error_Logs()
                {
                    ErrorMessage = exception.ToString(),
                    StackTrace = exception.StackTrace,
                    Type = area,
                    LoggedTime = DateTime.Now
                });
                Db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
