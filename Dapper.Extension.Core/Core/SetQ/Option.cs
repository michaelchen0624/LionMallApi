using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core
{
    public abstract class Option<T>:Query<T>,IOption<T>
    {
        protected Option(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {

        }

        protected Option(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {

        }

        /// <inheritdoc />
        public virtual Query<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            SqlProvider.SetContext.SelectExpression = selector;

            return new QuerySet<TResult>(DbCon, SqlProvider, typeof(T), DbTransaction);
        }

        /// <inheritdoc />
        public virtual Option<T> Top(int num)
        {
            SqlProvider.SetContext.TopNum = num;
            return this;
        }
    }
}
