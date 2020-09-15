using System;
using System.IO;
using System.Web;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.BussinessBase
{
    public class BaseLogic : IDisposable
    {
        Entities _dbContext;
        private static readonly object padlock = new object();
        
        public Entities Db
        {
            get
            {
                if (_dbContext == null)
                {
                    lock (padlock)
                    {
                        if (_dbContext == null)
                            _dbContext = new Entities();
                    }
                }
                return _dbContext;
            }
        }
        
        public void Dispose()
        {
            this.Dispose();
        }

        public void AddNewImage(string ImageSavePath, string name)
        {
            var postedFile = HttpContext.Current.Request.Files[0];
            postedFile.SaveAs(Path.Combine(ImageSavePath, name));
        }
    }
}
