using Lion.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionMallApi
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiResultFilter:IResultFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is BadRequestObjectResult)
            {
                var lang = context.HttpContext.Request.Headers["lang"].ToString();
                if (string.IsNullOrWhiteSpace(lang))
                    lang = "cn";
                BadRequestObjectResult res = (BadRequestObjectResult)context.Result;
                var obj = res.Value as ValidationProblemDetails;
                StringBuilder sb = new StringBuilder();
                string msg = string.Empty;
                foreach (var info in obj.Errors)
                {
                    msg = info.Value[0];
                    break;
                }
                context.Result = new JsonResult(geneRetData.geneRate<string>(1401, "", 
                    LangServiceStatic.GetLangByLangAndTitle(msg,lang)));
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }
    }
}
