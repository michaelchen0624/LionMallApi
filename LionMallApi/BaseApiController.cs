using Microsoft.AspNetCore.Mvc;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionMallApi
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseApiController:ControllerBase
    {
        private string _lang;
        //private string _platform;
        ///// <summary>
        ///// 
        ///// </summary>
        //public string platform
        //{
        //    get
        //    {
        //        var useragent = Request.Headers["User-Agent"].ToString();
        //        if (useragent.Contains("android", StringComparison.OrdinalIgnoreCase))
        //        {
        //            _platform = "android";
        //        }
        //        else if (useragent.Contains("iphone", StringComparison.OrdinalIgnoreCase))
        //        {
        //            _platform = "ios";
        //        }
        //        else
        //            _platform = "notfound";
        //        return _platform;
        //    }
        //}
        /// <summary>
        /// 语言
        /// </summary>
        public string lang
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_lang))
                {
                    var tmplang = HttpContext.Request.Headers["lang"].ToString();
                    if (string.IsNullOrWhiteSpace(tmplang))
                        _lang = "cn";
                    else
                        _lang = tmplang;
                }
                return _lang;
            }
        }
        private UserTicket _ticket;
        /// <summary>
        /// 
        /// </summary>
        public UserTicket ticket
        {
            get
            {
                if (_ticket == null)
                {
                    var head = HttpContext.Request.Headers;
                    _ticket = new UserTicket();
                    _ticket.ticketCode = head["ticketCode"].ToString();
                    _ticket.timeStamp = head["timeStamp"].ToString();
#if DEBUG                    
                    _ticket.userId = "9489cacf8e4f4696a9972df3d9f9fb19";
#else
                    _ticket.userId = head["userId"].ToString();
#endif
                }
                return _ticket;
            }

        }
    }
}
