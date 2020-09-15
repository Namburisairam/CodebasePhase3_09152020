using System;
using System.Linq.Expressions;

namespace EventManagement.BusinessLogic.Interfaces
{
    public interface IEntityValidity<T>
    {
        Expression<Func<T, bool>> CheckValidity { get; }
    }
}
