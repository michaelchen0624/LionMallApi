using Dapper.Extension.Core.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core
{
    public class QuerySet<T> : Aggregation<T>, IQuerySet<T>
    {
        public QuerySet(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        public QuerySet(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        internal QuerySet(IDbConnection conn, SqlProvider sqlProvider, Type tableType, IDbTransaction dbTransaction) : base(conn, sqlProvider, dbTransaction)
        {
            SqlProvider.SetContext.TableType = tableType;
        }

        public QuerySet<T> Where(Expression<Func<T, bool>> predicate)
        {
            SqlProvider.SetContext.WhereExpression = SqlProvider.SetContext.WhereExpression == null ? predicate : ((Expression<Func<T, bool>>)SqlProvider.SetContext.WhereExpression).And(predicate);

            return this;
        }

        public QuerySet<T> WithNoLock()
        {
            SqlProvider.SetContext.NoLock = true;
            return this;
        }
    }
}
