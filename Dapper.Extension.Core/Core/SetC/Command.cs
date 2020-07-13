using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Core
{
    public  class Command<T>:AbstractSet, ICommand<T>,IInsert<T>
    {
        //public SqlProvider SqlProvider { get; protected set; }
        //public IDbConnection DbCon { get; protected set; }
        //public IDbTransaction DbTransaction { get; protected set; }

        protected Command(SqlProvider sqlProvider, IDbConnection dbCon, IDbTransaction dbTransaction) : base(dbCon, sqlProvider, dbTransaction)
        {

        }

        protected Command(SqlProvider sqlProvider, IDbConnection dbCon) : base(dbCon, sqlProvider)
        {

        }
        public int Update(T entity)
        {
            SqlProvider.FormatUpdate(entity);

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task<int> UpdateAsync(T entity)
        {
            SqlProvider.FormatUpdate(entity);

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public int Update(Expression<Func<T, T>> updateExpression)
        {
            SqlProvider.FormatUpdate(updateExpression);

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task<int> UpdateAsync(Expression<Func<T, T>> updateExpression)
        {
            SqlProvider.FormatUpdate(updateExpression);

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public int Delete()
        {
            SqlProvider.FormatDelete();

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task<int> DeleteAsync()
        {
            SqlProvider.FormatDelete();

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public int Insert(T entity)
        {
            SqlProvider.FormatInsert(entity);

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }
        public int Insert(T entity, bool getIdentity = false)
        {
            SqlProvider.FormatInsert(entity, getIdentity);
            if (getIdentity)
                return DbCon.ExecuteScalar<int>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }
        public async Task<int> InsertAsync(T entity)
        {
            SqlProvider.FormatInsert(entity);

            return await DbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public ICommand<T> Incre(Expression<Func<T, T>> incre)
        {
            SqlProvider.SetContext.IncreExpression = incre;
            return this;
        }
        public int Sql(string sql,object parm)
        {
            return DbCon.Execute(sql, parm,DbTransaction);
        }

        public int UpdateOnlyIncre()
        {
            SqlProvider.FormatUpdateOnlyIncre<T>();

            return DbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }
    }
}
