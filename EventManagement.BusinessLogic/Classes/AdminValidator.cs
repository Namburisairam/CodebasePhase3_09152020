using System;
using System.Linq.Expressions;
using EventManagement.BusinessLogic.Interfaces;
using EventManagement.DataAccess.DataBase.Model;

namespace EventManagement.BusinessLogic.Classes
{
    public class AdminValidator : IEntityValidity<Attende>
    {
        public Expression<Func<Attende, bool>> CheckValidity
        {
            get { return x => x.IsAdmin; }
        }
    }
}
