using Microsoft.AspNetCore.Http;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace LionMallApi
{
    /// <summary>
    /// 
    /// </summary>
    public class LionMallExceptionHandlerMiddleWare
    {
        private readonly RequestDelegate next;
        private ILogService _Log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="Log"></param>
        public LionMallExceptionHandlerMiddleWare(RequestDelegate next, ILogService Log)
        {
            this.next = next;
            this._Log = Log;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            await WriteExceptionAsync(context, exception).ConfigureAwait(false);
        }

        private async Task WriteExceptionAsync(HttpContext context, Exception exception)
        {

            HttpResponse response = context.Response;

            int nCode = 0;
            var msg = string.Empty;
            var dtex = (exception is LionException);
            if (dtex)
            {
                nCode = ((LionException)exception).Code;
            }
            else if (exception is Exception)
            {
                nCode = 500;
            }
            //response.ContentType = context.Request.Headers["Accept"];
            //TODO:logo
            
            if (dtex)
            {
                _Log.I("info", exception.Message);
            }               
            else
                _Log.E("err", exception.ToString());
            if(exception.Message.Contains("u_BalanceLessZero"))
            {
                nCode = 1401;
                msg = "余额不足";
            }
            response.ContentType = "application/json";
            await response.WriteAsync(string.Format(@"{{""status"":0,""code"":{0},""msg"":""{1}""  }}", nCode,
                string.IsNullOrWhiteSpace(msg) ? exception.Message : msg)).ConfigureAwait(false);
        }
    }
}
