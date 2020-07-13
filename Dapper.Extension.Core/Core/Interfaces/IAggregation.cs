using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core
{
    public interface IAggregation<T>
    {
        /// <summary>
        /// 条数
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// 总和
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sumExpression"></param>
        /// <returns></returns>
        TResult Sum<TResult>(Expression<Func<T, TResult>> sumExpression);
    }
}
