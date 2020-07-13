using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core.Model
{
    public class SetContext
    {
        public SetContext()
        {
            OrderbyExpressionList = new Dictionary<EOrderBy, LambdaExpression>();
        }

        public Type TableType { get; internal set; }

        public LambdaExpression WhereExpression { get; internal set; }

        /// <summary>
        /// 自增表达式用于设置字段=字段+值
        /// </summary>
        public LambdaExpression IncreExpression { get; internal set; }

        public LambdaExpression IfNotExistsExpression { get; internal set; }

        public Dictionary<EOrderBy, LambdaExpression> OrderbyExpressionList { get; internal set; }

        public LambdaExpression SelectExpression { get; internal set; }

        public int? TopNum { get; internal set; }

        public bool NoLock { get; internal set; }
    }
}
