using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core
{
    public interface IQuerySet<T>
    {
        QuerySet<T> Where(Expression<Func<T, bool>> predicate);

        QuerySet<T> WithNoLock();
    }
}
