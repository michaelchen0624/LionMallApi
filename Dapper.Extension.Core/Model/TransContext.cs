using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dapper.Extension.Core
{
    public class TransContext
    {
        private readonly IDbConnection _sqlConnection;

        private readonly IDbTransaction _dbTransaction;

        //private readonly SqlProvider _sqlProvider;

        private Func<SqlProvider> _createProvider;

        public TransContext(IDbConnection sqlConnection, IDbTransaction dbTransaction, Func<SqlProvider> creatProvider)
        {
            _sqlConnection = sqlConnection;
            _dbTransaction = dbTransaction;
            _createProvider = creatProvider;
        }
        public QuerySet<T> QuerySet<T>()
        {
            var sqlprovider = _createProvider();
            return new QuerySet<T>(_sqlConnection,sqlprovider, _dbTransaction);
        }

        public CommandSet<T> CommandSet<T>()
        {
            var sqlprovider = _createProvider();
            return new CommandSet<T>(_sqlConnection, sqlprovider, _dbTransaction);
        }
    }
}
