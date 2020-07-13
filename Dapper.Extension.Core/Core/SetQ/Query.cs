using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Dapper.Extension.Core
{
    public class Query<T> :AbstractSet, IQuery<T>
    {
        //public SqlProvider SqlProvider { get; protected set; }
        //public IDbConnection DbCon { get; protected set; }
        //public IDbTransaction DbTransaction { get; protected set; }
        protected Query(IDbConnection dbCon, SqlProvider sqlProvider) : base(dbCon, sqlProvider)
        {
        }
        protected Query(IDbConnection dbCon, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(dbCon, sqlProvider, dbTransaction)
        {
        }

        public T Get()
        {
            SqlProvider.FormatGet<T>();

            return DbCon.QueryFirstOrDefault<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task<T> GetAsync()
        {
            SqlProvider.FormatGet<T>();

            return await DbCon.QueryFirstOrDefaultAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public IEnumerable<T> ToList()
        {
            SqlProvider.FormatToList<T>();

            return DbCon.Query<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public async Task<IEnumerable<T>> ToListAsync()
        {
            SqlProvider.FormatToList<T>();

            return await DbCon.QueryAsync<T>(SqlProvider.SqlString, SqlProvider.Params, DbTransaction);
        }

        public PageList<T> PageList(int pageIndex, int pageSize)
        {
            SqlProvider.FormatToPageList<T>(pageIndex, pageSize);

            using (var queryResult = DbCon.QueryMultiple(SqlProvider.SqlString, SqlProvider.Params, DbTransaction))
            {
                var pageTotal = queryResult.ReadFirst<int>();

                var itemList = queryResult.Read<T>();

                return new PageList<T>(pageIndex, pageSize, pageTotal, itemList);
            }
        }
        public PageList<T> SqlPageList(string sql,object param,int pageIndex,int pageCount)
        {
            using (var queryResult = DbCon.QueryMultiple(sql, param, DbTransaction))
            {
                var pageTotal = queryResult.ReadFirst<int>();

                var itemList = queryResult.Read<T>();

                return new PageList<T>(pageIndex, pageCount, pageTotal, itemList);
            }
        }
        public IEnumerable<T> SqlList(string sql,object parm)
        {
            return DbCon.Query<T>(sql, parm,DbTransaction);
        }
        public T SqlEntity(string sql,object parm)
        {
            return DbCon.Query<T>(sql, parm,DbTransaction).FirstOrDefault();
        }
        public (T, T2) SqlMultiple<T2>(string sql, object param)
        {
            using (var queryResult = DbCon.QueryMultiple(sql, param, DbTransaction))
            {
                var t = queryResult.Read<T>().FirstOrDefault();
                var t2 = queryResult.Read<T2>().FirstOrDefault();
                return (t, t2);
            }
        }
        public (T, T2, T3) SqlMultiple<T2, T3>(string sql, object param)
        {
            using (var queryResult = DbCon.QueryMultiple(sql, param, DbTransaction))
            {
                var t = queryResult.Read<T>().FirstOrDefault();
                var t2 = queryResult.Read<T2>().FirstOrDefault();
                var t3 = queryResult.Read<T3>().FirstOrDefault();
                return (t, t2, t3);
            }
        }
        public (T,T2,T3,T4) SqlMultiple<T2,T3,T4>(string sql,object param)
        {
            using (var queryResult = DbCon.QueryMultiple(sql,param, DbTransaction))
            {
                var t = queryResult.Read<T>().FirstOrDefault();
                var t2 = queryResult.Read<T2>().FirstOrDefault();
                var t3 = queryResult.Read<T3>().FirstOrDefault();
                var t4 = queryResult.Read<T4>().FirstOrDefault();
                return (t, t2, t3, t4);
            }
        }
    }
}
