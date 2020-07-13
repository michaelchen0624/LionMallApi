using Dapper.Extension.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using Dapper.Extension.MsSql.Helper;
using Dapper.Extension.Core.Expressions;

namespace Dapper.Extension.MsSql
{
    public class MsSqlProvider:SqlProvider
    {
        private const char OpenQuote = '[';
        private const char CloseQuote = ']';
        private const char ParameterPrefix = '@';
        private ResolveExpression resolveExpression;

        public MsSqlProvider()
        {
            ProviderOption = new ProviderOption(OpenQuote, CloseQuote, ParameterPrefix);
            resolveExpression = new ResolveExpression(ProviderOption);
        }

        protected sealed override ProviderOption ProviderOption { get; set; }

        public override SqlProvider FormatGet<T>()
        {
            var selectSql = resolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression, 1);

            var fromTableSql = FormatTableName();

            var nolockSql = resolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var orderbySql = resolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {orderbySql}";

            return this;
        }

        public override SqlProvider FormatToList<T>()
        {
            var topNum = SetContext.TopNum;

            var selectSql = resolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression, topNum);

            var fromTableSql = FormatTableName();

            var nolockSql = resolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            var orderbySql = resolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} {orderbySql}";

            return this;
        }

        public override SqlProvider FormatToPageList<T>(int pageIndex, int pageSize)
        {
            var orderbySql = resolveExpression.ResolveOrderBy(SetContext.OrderbyExpressionList);
            if (string.IsNullOrEmpty(orderbySql))
                throw new DapperExtensionException("order by takes precedence over pagelist");

            var selectSql = resolveExpression.ResolveSelect(typeof(T).GetProperties(), SetContext.SelectExpression, pageSize);

            var fromTableSql = FormatTableName();

            var nolockSql = resolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"SELECT COUNT(1) {fromTableSql} {nolockSql} {whereSql};";
            SqlString += $@"{selectSql}
            FROM    ( SELECT *
                      ,ROW_NUMBER() OVER ( {orderbySql} ) AS ROWNUMBER
                      {fromTableSql} {nolockSql}
                      {whereSql}
                    ) T
            WHERE   ROWNUMBER > {(pageIndex - 1) * pageSize}
                    AND ROWNUMBER <= {pageIndex * pageSize} {orderbySql};";

            return this;
        }

        public override SqlProvider FormatCount()
        {
            var selectSql = "SELECT COUNT(1)";

            var fromTableSql = FormatTableName();

            var nolockSql = resolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} ";

            return this;
        }

        public override SqlProvider FormatExists()
        {
            var selectSql = "SELECT TOP 1 1";

            var fromTableSql = FormatTableName();

            var nolockSql = resolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql}";

            return this;
        }

        public override SqlProvider FormatDelete()
        {
            var fromTableSql = FormatTableName();

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"DELETE {fromTableSql} {whereSql }";

            return this;
        }
        public override SqlProvider FormatInsert<T>(T entity,bool getIdentity=false)
        {
            var paramsAndValuesSql = FormatInsertParamsAndValues(entity);

            if (SetContext.IfNotExistsExpression == null)
                SqlString = $"INSERT INTO {FormatTableName(false)} ({paramsAndValuesSql[0]}) VALUES({paramsAndValuesSql[1]})";
            else
            {
                var ifnotexistsWhere = resolveExpression.ResolveWhere(SetContext.IfNotExistsExpression, "INT_");

                SqlString = string.Format(@"INSERT INTO {0}({1})  
                SELECT {2}
                WHERE NOT EXISTS(
                    SELECT 1
                    FROM {0}  
                {3}
                    ); ", FormatTableName(false), paramsAndValuesSql[0], paramsAndValuesSql[1], ifnotexistsWhere.SqlCmd);

                Params.AddDynamicParams(ifnotexistsWhere.Param);
            }
            if (getIdentity)
                SqlString = $"{SqlString} select @@identity";
            return this;
        }
        public override SqlProvider FormatInsert<T>(T entity)
        {
            var paramsAndValuesSql = FormatInsertParamsAndValues(entity);

            if (SetContext.IfNotExistsExpression == null)
                SqlString = $"INSERT INTO {FormatTableName(false)} ({paramsAndValuesSql[0]}) VALUES({paramsAndValuesSql[1]})";
            else
            {
                var ifnotexistsWhere = resolveExpression.ResolveWhere(SetContext.IfNotExistsExpression, "INT_");

                SqlString = string.Format(@"INSERT INTO {0}({1})  
                SELECT {2}
                WHERE NOT EXISTS(
                    SELECT 1
                    FROM {0}  
                {3}
                    ); ", FormatTableName(false), paramsAndValuesSql[0], paramsAndValuesSql[1], ifnotexistsWhere.SqlCmd);

                Params.AddDynamicParams(ifnotexistsWhere.Param);
            }
            return this;
        }
        public override SqlProvider FormatUpdateOnlyIncre<T>()
        {
            //var update = resolveExpression.ResolveUpdate(updateExpression);

            var where = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = where.SqlCmd;

            Params = where.Param;

            IncreExpression incre = null;
            if (SetContext.IncreExpression != null)
                incre = resolveExpression.ResolveIncreMent<T>(SetContext.IncreExpression);

            //Params.AddDynamicParams(update.Param);
            if (incre != null)
            {
                Params.AddDynamicParams(incre.Param);
                SqlString = $"UPDATE {FormatTableName(false)} set {incre.SqlCmd} {whereSql}";
            }
            else
            {
                //SqlString = $"UPDATE {FormatTableName(false)}  {whereSql}";
                throw new Exception($"incre is null");
            }


            return this;
        }
        public override SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression)
        {
            var update = resolveExpression.ResolveUpdate(updateExpression);

            var where = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = where.SqlCmd;

            Params = where.Param;

            IncreExpression incre=null;
            if (SetContext.IncreExpression != null)
                incre = resolveExpression.ResolveIncreMent<T>(SetContext.IncreExpression);

            Params.AddDynamicParams(update.Param);
            if (incre != null)
            {
                Params.AddDynamicParams(incre.Param);
                SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd},{incre.SqlCmd} {whereSql}";
            }
            else
            {
                SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {whereSql}";
            }
            

            return this;
        }

        public override SqlProvider FormatUpdate<T>(T entity)
        {
            var update = resolveExpression.ResolveUpdate<T>(a => entity);

            var where = resolveExpression.ResolveWhere(entity);

            var whereSql = where.SqlCmd;

            Params = where.Param;
            Params.AddDynamicParams(update.Param);

            SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {whereSql}";

            return this;
        }

        public override SqlProvider FormatSum<T>(LambdaExpression lambdaExpression)
        {
            var selectSql = resolveExpression.ResolveSum(typeof(T).GetProperties(), lambdaExpression);

            var fromTableSql = FormatTableName();

            var whereParams = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var nolockSql = resolveExpression.ResolveWithNoLock(SetContext.NoLock);

            var whereSql = whereParams.SqlCmd;

            Params = whereParams.Param;

            SqlString = $"{selectSql} {fromTableSql} {nolockSql} {whereSql} ";

            return this;
        }

        public override SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator)
        {
            var update = resolveExpression.ResolveUpdate(updator);

            var selectSql = resolveExpression.ResolveSelectOfUpdate(typeof(T).GetProperties(), SetContext.SelectExpression);

            var where = resolveExpression.ResolveWhere(SetContext.WhereExpression);

            var whereSql = where.SqlCmd;

            Params = where.Param;
            Params.AddDynamicParams(update.Param);

            var topNum = SetContext.TopNum;

            var topSql = topNum.HasValue ? $" TOP ({topNum.Value})" : "";
            SqlString = $"UPDATE {topSql} {FormatTableName(false)} WITH ( UPDLOCK, READPAST ) {update.SqlCmd} {selectSql} {whereSql}";

            return this;
        }

        public override SqlProvider ExcuteBulkCopy<T>(IDbConnection conn, IEnumerable<T> list)
        {
            BulkSqlHelper.BulkCopy(conn, list);
            return this;
        }
    }
}
