using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DataCommon
{
    public interface IRepository
    {
        DbParameter GenerateInParam(string paramName, SqlDbType sqlDbType, int size, object value);
        /// <summary>
        /// 返回实体类
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        T GetEntity<T>(string sql, string database="sqlserver", object param = null, IDbTransaction tran = null);
        /// <summary>
        /// 返回集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        IList<T> GetRepositoryList<T>(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null);

        /// <summary>
        /// 运行sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        int RepositoryExecute(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null);

        object ExecuteScalar(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null);

        int ExecuteStored(string StoredName, string database = "sqlserver", object param = null, IDbTransaction tran = null);
    }
}
