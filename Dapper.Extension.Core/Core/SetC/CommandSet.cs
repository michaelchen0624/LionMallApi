using Dapper.Extension.Core.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core
{
    public class CommandSet<T> : Command<T>, ICommandSet<T>
    {
        public CommandSet(IDbConnection conn, SqlProvider sqlProvider) : base(sqlProvider, conn)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        public CommandSet(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(sqlProvider, conn, dbTransaction)
        {
            SqlProvider.SetContext.TableType = typeof(T);
        }

        internal CommandSet(IDbConnection conn, SqlProvider sqlProvider, Type tableType, LambdaExpression whereExpression) : base(sqlProvider, conn)
        {
            SqlProvider.SetContext.TableType = tableType;
            SqlProvider.SetContext.WhereExpression = whereExpression;
        }
        //public ICommand<T> Incre(Expression<Func<T,T>> incre)
        //{
        //    SqlProvider.SetContext.IncreExpression = incre;
        //    return this;
        //}
        public ICommand<T> Where(Expression<Func<T, bool>> predicate)
        {
            SqlProvider.SetContext.WhereExpression = SqlProvider.SetContext.WhereExpression == null ? predicate : ((Expression<Func<T, bool>>)SqlProvider.SetContext.WhereExpression).And(predicate);

            return this;
        }

        public IInsert<T> IfNotExists(Expression<Func<T, bool>> predicate)
        {
            SqlProvider.SetContext.IfNotExistsExpression = SqlProvider.SetContext.IfNotExistsExpression == null ? predicate : ((Expression<Func<T, bool>>)SqlProvider.SetContext.IfNotExistsExpression).And(predicate);

            return this;
        }

        public void BatchInsert(IEnumerable<T> entities, int timeout = 120)
        {
            SqlProvider.ExcuteBulkCopy(DbCon, entities);
        }
    }
}
