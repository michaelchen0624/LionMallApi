using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.Entity;
using Lion.Services;
using Lion.ViewModel.requestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prod.Core;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : BaseApiController
    {
        private CommonService _commonService;
        private UserService _userService;
        private RegisterService _regService;
        private LangService _langService;
        private ILogService _log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonService"></param>
        /// <param name="userService"></param>
        /// <param name="regService"></param>
        /// <param name="langService"></param>
        /// <param name="log"></param>
        public RegisterController(CommonService commonService,UserService userService,
            RegisterService regService,LangService langService,ILogService log)
        {
            _commonService = commonService;
            _userService = userService;
            _regService = regService;
            _langService = langService;
            _log = log;
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        [HttpPost("Reg")]
        public ActionResult<RespData<UserTicket>> Reg(RegData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            var phone = $"{phonearea}{data.phone}";
            if (!_commonService.verifyMsg(phone, data.msgcode))
                return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("验证码不正确", lang));
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if(user!=null)
                return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("电话号码已注册", lang));           
            userEntity recomend = null;
            if(!string.IsNullOrWhiteSpace(data.recommend))
            {
                recomend = _userService.GetUserByRecommend(data.recommend);
                if(recomend==null)
                    return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("邀请码不正确", lang));
            }
            string guid = string.Empty;
            try
            {
                guid = _regService.RegUser(data.username, phonearea, data.phone,
                    data.paypwd, recomend);
            }
            catch(Exception ex)
            {
                _log.E("Reg", $"{ex.ToString()},提交内容:{JsonConvert.SerializeObject(data)}");
                return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("注册不成功", lang));
            }               
            //if (string.IsNullOrWhiteSpace(guid))
            return geneRetData.geneRate<UserTicket>(1, UserTicket.CreatTicket(guid));
        }
        /// <summary>
        /// 注册发送短信
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("SendRegMsg")]
        public ActionResult<RespData<bool>> SendRegMsg(msgPhoneData data)
        {
            //_log.I("test", "22");
            //_log.I("Test", JsonConvert.SerializeObject(data));
            //_log.I("Testhead", Request.Headers["access-token"].ToString());
            var phonearea = string.IsNullOrWhiteSpace(data.phonearea) ? "86"
                : StringHelper.GetNumberFromStr(data.phonearea);
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if(user!=null)
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("电话号码已注册", lang));
            var ret= _commonService.SendMsg(data.phone, 4, msgType.Register,phonearea, phonekey: true);
            if (ret.Contains("errmsg"))
            {
                var msg = ret.Split(":");
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle($"{msg[msg.Length - 1]}", lang));
            }                
            return geneRetData.geneRate<bool>(1, true);
        }

        /// <summary>
        /// 新注册
        /// </summary>
        /// <returns></returns>
        [HttpPost("GeneralReg")]
        public ActionResult<RespData<UserTicket>> GeneralReg(GeneralRegData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            var phone = $"{phonearea}{data.phone}";
            if (!_commonService.verifyMsg(phone, data.msgcode))
                return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("验证码不正确", lang));
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if (user != null)
                return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("电话号码已注册", lang));
            userEntity recomend = null;
            if (!string.IsNullOrWhiteSpace(data.recommend))
            {
                recomend = _userService.GetUserByRecommend(data.recommend);
                if (recomend == null)
                    return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("邀请码不正确", lang));
            }
            string guid = string.Empty;
            try
            {
                //guid = _regService.RegUser(data.username, phonearea, data.phone,
                //    data.paypwd, recomend);
                guid = _regService.NRegUser(phonearea, data.phone, recomend);
            }
            catch (Exception ex)
            {
                _log.E("Reg", $"{ex.ToString()},提交内容:{JsonConvert.SerializeObject(data)}");
                return geneRetData.geneRate<UserTicket>(1401, null,
                    _langService.GetLangByTitle("注册不成功", lang));
            }
            //if (string.IsNullOrWhiteSpace(guid))
            return geneRetData.geneRate<UserTicket>(1, UserTicket.CreatTicket(guid));
        }
    }
}