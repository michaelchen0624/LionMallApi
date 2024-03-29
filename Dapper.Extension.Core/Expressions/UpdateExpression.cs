﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extension.Core.Expressions
{
    public class UpdateExpression: ExpressionVisitor
    {
        #region sql指令

        private readonly StringBuilder _sqlCmd;

        private const string Prefix = "UPDATE_";

        /// <summary>
        /// sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.Length > 0 ? $" SET {_sqlCmd} " : "";

        private readonly ProviderOption _providerOption;

        private readonly char _parameterPrefix;

        public DynamicParameters Param { get; }

        #endregion

        #region 执行解析

        /// <inheritdoc />
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public UpdateExpression(LambdaExpression expression, ProviderOption providerOption)
        {
            _sqlCmd = new StringBuilder(100);
            _providerOption = providerOption;
            _parameterPrefix = _providerOption.ParameterPrefix;
            Param = new DynamicParameters();

            Visit(expression);
        }

        #endregion

        protected override System.Linq.Expressions.Expression VisitMember(MemberExpression node)
        {
            var memberInitExpression = node;

            var entity = ((ConstantExpression)TrimExpression.Trim(memberInitExpression)).Value;

            var properties = memberInitExpression.Type.GetProperties();

            foreach (var item in properties)
            {
                if (item.CustomAttributes.Any(b => b.AttributeType == typeof(KeyAttribute)))
                    continue;

                if (_sqlCmd.Length > 0)
                    _sqlCmd.Append(",");

                var paramName = item.Name;
                var value = item.GetValue(entity);
                var fieldName = _providerOption.CombineFieldName(item.GetColumnAttributeName());
                SetParam(fieldName, paramName, value);
            }

            return node;
        }


        protected override System.Linq.Expressions.Expression VisitMemberInit(MemberInitExpression node)
        {
            var memberInitExpression = node;

            foreach (var item in memberInitExpression.Bindings)
            {
                var memberAssignment = (MemberAssignment)item;

                if (_sqlCmd.Length > 0)
                    _sqlCmd.Append(",");

                var paramName = memberAssignment.Member.Name;
                var fieldName = _providerOption.CombineFieldName(memberAssignment.Member.GetColumnAttributeName());
                var value = memberAssignment.Expression.ToConvertAndGetValue();
                //var constantExpression = (ConstantExpression)memberAssignment.Expression;
                //SetParam(fieldName, paramName, constantExpression.Value);
                SetParam(fieldName, paramName, value);
            }

            return node;
        }

        private void SetParam(string fieldName, string paramName, object value)
        {
            var n = $"{_parameterPrefix}{Prefix}{paramName}";
            _sqlCmd.AppendFormat(" {0}={1} ", fieldName, n);
            Param.Add(n, value);
        }
    }
}
