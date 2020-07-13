using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Core
{
    public interface ICommand<T>
    {
        int Update(T entity);
        int UpdateOnlyIncre();

        Task<int> UpdateAsync(T entity);

        int Update(Expression<Func<T, T>> updateExpression);

        Task<int> UpdateAsync(Expression<Func<T, T>> updateExpression);

        int Delete();

        Task<int> DeleteAsync();

        ICommand<T> Incre(Expression<Func<T, T>> incre);
    }
}
