using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dapper.Extension.Core
{
    public abstract class AbstractSet
    {
        public SqlProvider SqlProvider { get; protected set; }
        public IDbConnection DbCon { get; protected set; }
        public IDbTransaction DbTransaction { get; protected set; }

        protected AbstractSet(IDbConnection dbCon, SqlProvider sqlProvider, IDbTransaction dbTransaction)
        {
            SqlProvider = sqlProvider;
            DbCon = dbCon;
            DbTransaction = dbTransaction;
        }

        protected AbstractSet(IDbConnection dbCon, SqlProvider sqlProvider)
        {
            SqlProvider = sqlProvider;
            DbCon = dbCon;
        }
    }
}
