using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Linq;
using Dapper.Extension.Core;
using System.Data;
using Dapper.Extension.MsSql;
using System.Linq.Expressions;

namespace DataCommon
{
    public class DataService
    {
        public static void Conn(Action<IDbConnection> action)
        {
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                action(Conn);
            }
        }
        public static int Insert<T>(T t)
        {
            int ret = 0;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                ret = Conn.CommandSet<T>().Insert(t);
            }
            return ret;
        }
        public static int Insert<T>(T t, Expression<Func<T, bool>> ifNotWhere)
        {
            int ret = 0;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                ret = Conn.CommandSet<T>().IfNotExists(ifNotWhere).Insert(t);
            }
            return ret;
        }
        public static T Get<T, TProperty>(Expression<Func<T, bool>> where,
            Expression<Func<T, TProperty>> field, EOrderBy orderBy = EOrderBy.Asc)
        {
            T t;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                if (orderBy == EOrderBy.Asc)
                    t = Conn.QuerySet<T>().Where(where).OrderBy(field).Get();
                else
                    t = Conn.QuerySet<T>().Where(where).OrderByDescing(field).Get();
            }
            return t;
        }
        public static T Get<T>(Expression<Func<T, bool>> where)
        {
            T t;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                t = Conn.QuerySet<T>().Where(where).Get();
            }
            return t;
        }
        public static int GetCount<T>(Expression<Func<T, bool>> where)
        {
            var database = "sqlserver";
            int count = 0;
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                count = Conn.QuerySet<T>().Where(where).Count();
            }
            return count;
        }

        public static int GetCount(string sql, object param)
        {
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return (int)Conn.ExecuteScalar(sql, param);
            }
        }
        public static IEnumerable<T> GetList<T>(Expression<Func<T, bool>> where = null)
        {
            IEnumerable<T> list;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                if (where == null)
                    list = Conn.QuerySet<T>().ToList();
                else
                    list = Conn.QuerySet<T>().Where(where).ToList();
            }
            return list;
        }
        public static IEnumerable<T> GetList<T, TProperty>(Expression<Func<T, bool>> where,
            Expression<Func<T, TProperty>> field, EOrderBy orderBy = EOrderBy.Asc)
        {
            IEnumerable<T> list;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                if (orderBy == EOrderBy.Asc)
                    list = Conn.QuerySet<T>().Where(where).OrderBy(field).ToList();
                else
                    list = Conn.QuerySet<T>().Where(where).OrderByDescing(field).ToList();
            }
            return list;
        }
        public static int Update<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> update)
        {
            var database = "sqlserver";
            int count = 0;
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                count = Conn.CommandSet<T>().Where(where).Update(update);
            }
            return count;
        }
        public static int UpdateIncre<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> update)
        {
            var database = "sqlserver";
            int count = 0;
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                count = Conn.CommandSet<T>().Where(where).Incre(update).UpdateOnlyIncre();
            }
            return count;
        }
        public static IEnumerable<T> QueryListBySql<T>(string sql,object param)
        {
            IEnumerable<T> t;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                t = Conn.QuerySet<T>().SqlList(sql, param);
            }
            return t;
        }
        public static T QueryEntityBySql<T>(string sql, object param)
        {
            T t;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                t = Conn.QuerySet<T>().SqlEntity(sql, param);
            }
            return t;
        }

        public static IEnumerable<T> QueryPageListBySql<T>(string sql,object param,int pageIndex,int pageCount)
        {
            PageList<T> pagelist;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                pagelist = Conn.QuerySet<T>().SqlPageList(sql, param,pageIndex,pageCount);
            }
            return pagelist.Items;
        }
        public static IEnumerable<T> GetPageList<T, TProperty>(Expression<Func<T, bool>> where,
            Expression<Func<T, TProperty>> field, EOrderBy orderBy = EOrderBy.Asc,
            int pageIndex = 1, int pageCount = 10)
        {
            PageList<T> pagelist;
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                if (orderBy == EOrderBy.Asc)
                    pagelist = Conn.QuerySet<T>().Where(where).OrderBy(field).PageList(pageIndex, pageCount);
                else
                    pagelist = Conn.QuerySet<T>().Where(where).OrderByDescing(field).PageList(pageIndex, pageCount);
            }
            return pagelist.Items;
        }
        public static (T, T2) GetMultiple<T, T2>(string sql, object param)
        {
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.QuerySet<T>().SqlMultiple<T2>(sql, param);
            }
        }
        public static (T, T2, T3) GetMultiple<T, T2, T3>(string sql, object param)
        {
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.QuerySet<T>().SqlMultiple<T2, T3>(sql, param);
            }
        }
        public static (T,T2,T3,T4) GetMultiple<T,T2,T3,T4>(string sql,object param)
        {
            var database = "sqlserver";
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.QuerySet<T>().SqlMultiple<T2, T3, T4>(sql, param);
            }
        }
        public static void Transcation(Action<TransContext> action)
        {
            var database = "sqlserver";
            var Conn = RDBSHelper.GetDbConn(database);
            Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
            if (Conn.State == ConnectionState.Closed)
                Conn.Open();
            var transaction = Conn.BeginTransaction();
            try
            {
                action(new TransContext(Conn, transaction, () =>
                {
                    return new MsSqlProvider();
                }));
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                //if (exAction != null)
                //    exAction(ex);
                //else
                //{
                //transaction.Rollback();
                //}
                transaction.Rollback();
                throw;
            }
            finally
            {
                Conn.Close();
            }
        }
        //public static int Insert<T>(T t, bool getIdentity = false)
        //{
        //    var database = "sqlserver";
        //    using (var Conn = RDBSHelper.GetDbConn(database))
        //    {
        //        Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
        //        return Conn.CommandSet<T>().Insert(t,getIdentity);
        //    }
        //}
        //public static void Transcation(Action<TransContext> action)
        //{
        //    var database = "sqlserver";
        //    using (var Conn = RDBSHelper.GetDbConn(database))
        //    {
        //        Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
        //        Conn.Transaction(action);
        //    }
        //}
        //public static T GetEntity<T>(string sql,object param=null)
        //{
        //    var database = "sqlserver";
        //    using (var Conn = RDBSHelper.GetDbConn(database))
        //    {
        //        Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
        //        return Conn.Query<T>(sql, param).FirstOrDefault();
        //    }
        //}
        //public static IList<T> GetList<T>(string sql,object param=null)
        //{
        //    var database = "sqlserver";
        //    using (var Conn = RDBSHelper.GetDbConn(database))
        //    {
        //        Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
        //        return Conn.Query<T>(sql, param).ToList();
        //    }
        //}
    }
}
