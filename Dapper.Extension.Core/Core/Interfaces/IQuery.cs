using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Core
{
    public interface IQuery<T>
    {
        T Get();

        Task<T> GetAsync();

        IEnumerable<T> ToList();

        Task<IEnumerable<T>> ToListAsync();

        PageList<T> PageList(int pageIndex, int pageSize);
    }
}
