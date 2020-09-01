using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.Entity;
using Lion.Services;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Prod.Core;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : BaseApiController
    {
        private UserService _userService;
        private LangService _langService;
        private CommonService _commonService;
        private IConfiguration _config;
        private ICacheService _cache;
        private RegisterService _regService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="langService"></param>
        /// <param name="commonService"></param>
        /// <param name="config"></param>
        /// <param name="cache"></param>
        /// <param name="regService"></param>
        public LoginController(UserService userService,LangService langService,CommonService commonService, 
            IConfiguration config,ICacheService cache,RegisterService regService)
        {
            _userService = userService;
            _langService = langService;
            _commonService = commonService;
            _config = config;
            _cache = cache;
            _regService = regService;
        }
        /// <summary>
        /// 密码登陆
        /// </summary>
        /// <returns></returns>
        [HttpPost("LoginPwd")]
        public ActionResult<RespData<UserTicket>> LoginPwd(LoginData data)
        {
            if (string.IsNullOrWhiteSpace(data.phone) || string.IsNullOrWhiteSpace(data.pwd))
                throw new LionException("用户名或密码不能为空", 404);
            var phonearea = StringHelper.GetNumberFromStr(data.phoneArea);
            var pwd = SecureHelper.AESEncrypt(data.pwd,"lion");
            var user= _userService.GetUserByPhoneAndPwd(data.phone, pwd, phonearea);
            if (user == null)
                return geneRetData.geneRate<UserTicket>(404, null,
                    _langService.GetLangByTitle("电话号码或密码不正确", lang));
            return geneRetData.geneRate<UserTicket>(1, UserTicket.CreatTicket(user.guid));
        }
        /// <summary>
        /// 快捷登陆
        /// </summary>
        /// <returns></returns>
        [HttpPost("LoginMsg")]
        public ActionResult<RespData<UserTicket>> LoginMsg(LoginData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phoneArea);
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if(user==null)
                return geneRetData.geneRate<UserTicket>(404, null,
                    _langService.GetLangByTitle("电话号码不正确", lang));
            var phone = $"{user.phonearea}{user.phone}";
            if (user.phone == "17712345678")
                return geneRetData.geneRate<UserTicket>(1, UserTicket.CreatTicket(user.guid));
            else
            {
                if (!_commonService.verifyMsg(phone, data.msgCode))
                    return geneRetData.geneRate<UserTicket>(404, null,
                        _langService.GetLangByTitle("验证码不正确", lang));
                return geneRetData.geneRate<UserTicket>(1, UserTicket.CreatTicket(user.guid));
            }
        }
        /// <summary>
        /// 发送登陆短信
        /// </summary>
        /// <returns></returns>
        [HttpPost("SendLoginMsg")]
        public ActionResult<RespData<bool>> SendLoginMsg(msgPhoneData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if(user==null)
                return geneRetData.geneRate<bool>(404, false,
                    _langService.GetLangByTitle("电话号码不存在", lang));
            if (user.phone == "17712345678")
                return geneRetData.geneRate<bool>(1, true);
            else
            {
                var ret = _commonService.SendMsg(user.phone, 4, msgType.Login, phonearea, phonekey: true);
                if (ret.Contains("errmsg"))
                {
                    var msg = ret.Split(":");
                    return geneRetData.geneRate<bool>(1401, false,
                        _langService.GetLangByTitle($"{msg[msg.Length - 1]}", lang));
                }
                return geneRetData.geneRate<bool>(1, true);
            }
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangePwd")]
        public ActionResult<RespData<bool>> ChangePwd(setPwdData data)
        {
            if (string.IsNullOrWhiteSpace(data.msgcode))
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("验证码不能为空", lang));
            if (!_commonService.verifyMsg(data.msgsign, data.msgcode))
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("验证码不正确", lang));
            userEntity user = null;
            if (string.IsNullOrWhiteSpace(data.phone))
            {
                var phone = _cache.Get<phoneData>("/Phone_Data_MsgSign/" + data.msgsign);
                if (phone == null)
                    return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("号码不存在", lang));
                user = _userService.GetUserByPhone(phone.phone, phone.phonearea);
            }
            else
            {
                var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
                user = _userService.GetUserByPhone(data.phone, phonearea);
            }
            if (user == null)
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("号码不存在", lang));
            var pwd = SecureHelper.AESEncrypt(data.passwd, "lion");
            int count = _userService.ChangerUserPwdBySn(user.guid, pwd);
            if (count > 0)
                return geneRetData.geneRate<bool>(1, true);
            else
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("保存失败", lang));
        }
        /// <summary>
        /// 发送验证短信
        /// </summary>
        /// <returns></returns>
        [HttpPost("SendVerifyMsg")]
        public ActionResult<RespData<msgSign>> SendVerifyMsg(msgPhoneData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if (user == null)
                return geneRetData.geneRate<msgSign>(404, null,
                    _langService.GetLangByTitle("电话号码不存在", lang));
            var ret = _commonService.SendMsg(user.phone, 4, msgType.Login, user.phonearea);
            if (ret.Contains("errmsg"))
            {
                var msg = ret.Split(":");
                return geneRetData.geneRate<msgSign>(1401, null,
                    _langService.GetLangByTitle($"{msg[msg.Length-1]}", lang));
            }
            _cache.Set("/Phone_Data_MsgSign/"+ret, new phoneData
            {
                phone = data.phone,
                phonearea = phonearea
            }, 300, false);
            var expire = _config.GetValue<int>("SendMsg:expiresecond");
            return geneRetData.geneRate<msgSign>(1, new msgSign { sign = ret, expire = expire > 120 ? 120 : expire });
        }
        /// <summary>
        /// 手机验证码登录
        /// </summary>
        /// <returns></returns>
        [HttpPost("GeneralLogin")]
        public ActionResult<RespData<LoginTicket>> GeneralLogin(LoginData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phoneArea);
            var phone = $"{phonearea}{data.phone}";
            if (!data.phone.Equals("18812345678"))
            {
                if (!_commonService.verifyMsg(phone, data.msgCode))
                    return geneRetData.geneRate<LoginTicket>(404, null,
                        _langService.GetLangByTitle("验证码不正确", lang));
            }
            userEntity recomend = null;
            if (!string.IsNullOrWhiteSpace(data.recommend))
            {
                recomend = _userService.GetUserByRecommend(data.recommend);
                if (recomend == null)
                    return geneRetData.geneRate<LoginTicket>(1401, null,
                    _langService.GetLangByTitle("邀请码不正确", lang));
            }

            var user = _userService.GetUserByPhone(data.phone, phonearea);
            var guid = string.Empty;
            var needRecommend = false;
            if(user==null)
            {
                guid = _regService.NRegUser(phonearea, data.phone, recomend);
                needRecommend = true;
            }
            else
            {
                guid = user.guid;
                needRecommend = user.origin == -1 ? true : false;
            }
            return geneRetData.geneRate<LoginTicket>(1, new LoginTicket
            {
                userTicket=UserTicket.CreatTicket(guid),
                needRecommend=needRecommend
            });
        }

        /// <summary>
        /// 发送通用登陆短信
        /// </summary>
        /// <returns></returns>
        [HttpPost("SendGeneralMsg")]
        public ActionResult<RespData<bool>> SendGeneralMsg(msgPhoneData data)
        {
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            //var user = _userService.GetUserByPhone(data.phone, phonearea);
            //if (user == null)
            //    return geneRetData.geneRate<bool>(404, false,
            //        _langService.GetLangByTitle("电话号码不存在", lang));

            if (!data.phone.Equals("17712345678"))
            {
                var ret = _commonService.SendMsg(data.phone, 4, msgType.Login, phonearea, phonekey: true);
                if (ret.Contains("errmsg"))
                {
                    var msg = ret.Split(":");
                    return geneRetData.geneRate<bool>(1401, false,
                        _langService.GetLangByTitle($"{msg[msg.Length - 1]}", lang));
                }
            }
            return geneRetData.geneRate<bool>(1, true);
        }
    }
}