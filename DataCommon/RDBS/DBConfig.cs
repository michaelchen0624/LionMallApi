using System;
using System.Collections.Generic;
using System.Text;

namespace DataCommon
{
    public class DBConfig
    {
        private static RdbsConfig rconfig = null;
        static DBConfig()
        {
            //var dbconfig = DBConfigUtils.Configuration;
            rconfig = new RdbsConfig()
            {
#if DEBUG
                RDBSConnectString = DBConfigUtils.Configuration["SqlServerConfig:dConnString"],
#else
                RDBSConnectString = DBConfigUtils.Configuration["SqlServerConfig:ConnString"],
#endif
                RDBSTablePre = DBConfigUtils.Configuration["SqlServerConfig:RDBSTablePre"],
                DataBase= DBConfigUtils.Configuration["SqlServerConfig:DataBase"],
                BusinessModule= DBConfigUtils.Configuration["SqlServerConfig:BusinessModule"]
            };
        }
        public static RdbsConfig RDBSConfig
        {
            get { return rconfig; }
        }
    }
    public class RdbsConfig
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string RDBSConnectString { get; set; }
        /// <summary>
        /// 数据表前缀
        /// </summary>
        public string RDBSTablePre { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DataBase { get; set; }
        /// <summary>
        /// 业务模块名称
        /// </summary>
        public string BusinessModule { get; set; }
    }
    
}
