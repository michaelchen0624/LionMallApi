using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using System.Linq;

namespace DataCommon
{
    public class RepositoryBase:IRepository
    {
        #region 辅助方法

        /// <summary>
        /// 生成输入参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="sqlDbType">参数类型</param>
        /// <param name="size">类型大小</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public DbParameter GenerateInParam(string paramName, SqlDbType sqlDbType, int size, object value)
        {
            return GenerateParam(paramName, sqlDbType, size, ParameterDirection.Input, value);
        }

        /// <summary>
        /// 生成输出参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="sqlDbType">参数类型</param>
        /// <param name="size">类型大小</param>
        /// <returns></returns>
        public DbParameter GenerateOutParam(string paramName, SqlDbType sqlDbType, int size)
        {
            return GenerateParam(paramName, sqlDbType, size, ParameterDirection.Output, null);
        }

        /// <summary>
        /// 生成返回参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="sqlDbType">参数类型</param>
        /// <param name="size">类型大小</param>
        /// <returns></returns>
        public DbParameter GenerateReturnParam(string paramName, SqlDbType sqlDbType, int size)
        {
            return GenerateParam(paramName, sqlDbType, size, ParameterDirection.ReturnValue, null);
        }

        /// <summary>
        /// 生成参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="sqlDbType">参数类型</param>
        /// <param name="size">类型大小</param>
        /// <param name="direction">方向</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public DbParameter GenerateParam(string paramName, SqlDbType sqlDbType, int size, ParameterDirection direction, object value)
        {
            SqlParameter param = new SqlParameter(paramName, sqlDbType, size);
            param.Direction = direction;
            if (direction == ParameterDirection.Input && value != null)
                param.Value = value;
            return param;
        }
        #endregion
        public IDbTransaction Transaction { get; set; }
        public RepositoryBase()
        {
            // Conn = RDBSHelper.GetDbConn();
        }
        /// <summary>
        /// 返回实体类
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T GetEntity<T>(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null)
        {
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.Query<T>(sql, param,tran).FirstOrDefault();
            }

        }
        public IList<T> GetRepositoryList<T>(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null)
        {
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.Query<T>(sql, param,tran).ToList();
            }

        }
        public int RepositoryExecute(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null)
        {
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.Execute(sql, param, tran);
            }
        }
        public object ExecuteScalar(string sql, string database = "sqlserver", object param = null, IDbTransaction tran = null)
        {
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.ExecuteScalar(sql, param, tran);
            }
        }
        public int ExecuteStored(string StoredName, string database = "sqlserver", object param = null, IDbTransaction tran = null)
        {
            using (var Conn = RDBSHelper.GetDbConn(database))
            {
                Conn.ConnectionString = DBConfig.RDBSConfig.RDBSConnectString;
                return Conn.Execute(StoredName, param, tran, null, CommandType.StoredProcedure);
            }
        }
    }
}
