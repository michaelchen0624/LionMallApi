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
    /// 用户信息
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private UserService _userService;
        private CommonService _commonService;
        private LangService _langService;
        private IConfiguration _config;
        private PaymentService _payment;
        private TicketService _ticketService;
        private ICacheService _cache;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="commonService"></param>
        /// <param name="langService"></param>
        /// <param name="config"></param>
        /// <param name="payment"></param>
        /// <param name="ticketService"></param>
        /// <param name="cache"></param>
        public UserController(UserService userService,CommonService commonService,
            LangService langService,IConfiguration config,PaymentService payment,
            TicketService ticketService,ICacheService cache)
        {
            _userService = userService;
            _commonService = commonService;
            _langService = langService;
            _config = config;
            _payment = payment;
            _ticketService = ticketService;
            _cache = cache;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="timeStamp"></param>
        /// <param name="ticketCode"></param>
        /// <returns></returns>
        [HttpPost("GetUser")]
        public ActionResult<RespData<userInfo>> GetUser([FromHeader] string userId,[FromHeader] string timeStamp,[FromHeader] string ticketCode)
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            if (user == null)
                return geneRetData.geneRate<userInfo>(1, null);
            return geneRetData.geneRate<userInfo>(1, new userInfo
            {
                fullname=user.fullname,
                phone=user.phone,
                phonearea=user.phonearea,
                userName=user.username,
                userSN=user.guid
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("GetUserName")]
        public ActionResult<RespData<respUserName>> GetUserName(msgPhoneData data)
        {
            if (string.IsNullOrWhiteSpace(data.phone) || string.IsNullOrWhiteSpace(data.phonearea))
                throw new LionException("电话或区号不能为空", 404);
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            var user= _userService.GetUserByPhone(data.phone, phonearea);
            if (user == null)
                return geneRetData.geneRate<respUserName>(1401, null,
                    _langService.GetLangByTitle("找不到用户", lang));
            return geneRetData.geneRate<respUserName>(1, new respUserName
            {
                userName=user.username
            });
        }
        /// <summary>
        /// 个人中心
        /// </summary>
        /// <returns></returns>
        [HttpPost("UserCenter")]
        public ActionResult<RespData<userCenterViewModel>> UserCenter()
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            if(user==null)
                return geneRetData.geneRate<userCenterViewModel>(1, null);
            var userbalance = _userService.GetBalanceByUserSN(ticket.userId);
            var userextend = _userService.GetUserExtendByUid(user.uid);
            var totoal = userbalance.balance_reward + userbalance.balance_shopping - userbalance.frozen_reward -
                userbalance.frozen_shopping;
            var shopping = userbalance.balance_shopping - userbalance.frozen_shopping;
            var iconUrl = string.IsNullOrWhiteSpace(user.iconurl) ? "" : $"http://img.finipics.com{user.iconurl}";
            var url_list = new List<urlInfo>();

            var useragent = Request.Headers["User-Agent"].ToString();
            var iosFlag = useragent.Contains("iphone", StringComparison.OrdinalIgnoreCase);

            url_list.Add(new urlInfo
            {
                name = "wallet",
                url = iosFlag ? "http://hw_test.finipics.com/Wallet" : "http://h5.finipics.com/rewardCenter"
            });
            url_list.Add(new urlInfo
            {
                name= "business",
                url= "http://h5_test.finipics.com/business.html"
            });
            url_list.Add(new urlInfo
            {
                name = "usercenter",
                url = iosFlag ? "http://hw_test.finipics.com/User" : "http://h5.finipics.com/memberCenter"
            });
            return geneRetData.geneRate<userCenterViewModel>(1, new userCenterViewModel
            {
                total_balance=totoal.ToString("N4"),
                iconUrl=iconUrl,
                total_consume=userbalance.recharge_total.ToString("N2"),
                shopping_balance=shopping.ToString("N4"),
                userName=user.username,
                userLevel=userextend.level,
                urlList=url_list
            });
        }       
        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetPayPwd")]
        public ActionResult<RespData<bool>> SetPayPwd(setPwdData data)
        {
            if (string.IsNullOrWhiteSpace(data.msgcode))
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("验证码不能为空", lang));
            if (!_commonService.verifyMsg(data.msgsign, data.msgcode))
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("验证码不正确", lang));
            var pwd = SecureHelper.AESEncrypt(data.passwd, "lion");
            int count = _userService.ChangerUserPayPwdBySn(ticket.userId, pwd);
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
        public ActionResult<RespData<msgSign>> SendVerifyMsg()
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            if (user == null)
                throw new LionException("用户不存在", 404);
            var expire = _config.GetValue<int>("SendMsg:expiresecond");
            var ret= _commonService.SendMsg(user.phone, 6, msgType.Verify, user.phonearea,expireSecond:120);
            if (ret.Contains("errmsg"))
            {
                var msg = ret.Split(":");
                throw new LionException(_langService.GetLangByTitle($"{msg[msg.Length - 1]}", lang),
                    404);
            }                
            return geneRetData.geneRate<msgSign>(1, new msgSign { sign = ret, expire = expire > 120 ? 120 : expire });
        }
        /// <summary>
        /// 获取用户账户信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUserAccount")]
        public ActionResult<RespData<upaymentViewModel>> GetUserAccount(reqUsn data)
        {
            var user = _userService.GetUserByUserSN(data.usn);
            var upayment = _payment.GetActiveUPayMentListByUid(user.uid);//用户支付方式
            var paymentlist = new List<payment>();
            foreach (var item in upayment)//添加支付方式
            {
                paymentlist.Add(new payment
                {
                    accountNum = item.account,
                    payee = item.payee,
                    payTitle = lang == "cn" ? item.pay_cnname : item.pay_name,
                    qrCode = string.IsNullOrWhiteSpace(item.qrcode) ? item.qrcode : $"http://img.finipics.com{item.qrcode}",
                    payName = item.pay_name,
                    bank = item.bank,
                    payType = item.pay_type
                });
            }
            var model = new upaymentViewModel();
            model.p_list = paymentlist;
            model.usn = user.guid;
            return geneRetData.geneRate<upaymentViewModel>(1, model);
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetUserInfo")]
        public ActionResult<RespData<bool>> SetUserInfo(reqUserInfo data)
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var username = user.username;
            var icon = user.iconurl;
            var count = _userService.SetUserInfo(string.IsNullOrWhiteSpace(data.userName) ? username : data.userName,
                string.IsNullOrWhiteSpace(data.iconUrl) ? icon : data.iconUrl, ticket.userId);           
            return geneRetData.geneRate<bool>(1, count > 0 ? true : false);
        }
        /// <summary>
        /// 获取上传票据
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUpTicket")]
        public ActionResult<RespData<imgAuth>> GetUpTicket(upImgTypeData data)
        {
            var ret = _ticketService.GetUpTicket(data.imgType);
            return geneRetData.geneRate<imgAuth>(1, new imgAuth
            {
                Authorization = ret
            });
        }
        /// <summary>
        /// 支付密码
        /// </summary>
        /// <returns></returns>
        [HttpPost("HasPayPwd")]
        public ActionResult<RespData<bool>> HasPayPwd()
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            if (user == null)
                return geneRetData.geneRate<bool>(806, false, "");
            if(string.IsNullOrWhiteSpace(user.paypwd))
                return geneRetData.geneRate<bool>(806, false, "");
            return geneRetData.geneRate<bool>(1, true);
        }
        /// <summary>
        /// 设置推荐人
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetRecommend")]
        public ActionResult<RespData<bool>> SetRecommend(setInviteData data)
        {
            var user = _userService.GetUserByUserSN(data.usn);
            if (user == null)
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("用户不存在", lang));
            if(user.origin>=0)
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("已设置邀请码", lang));
            userEntity recommend = null;
            if(!string.IsNullOrWhiteSpace(data.inviteCode))
            {
                recommend= _userService.GetUserByRecommend(data.inviteCode);
                if(recommend==null)
                    return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("推荐码不正确", lang));
            }
            int count= _userService.SetRecommend(user, recommend);
            if(count<=0)
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("设置不成功", lang));
            return geneRetData.geneRate<bool>(1, true);
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
            if(string.IsNullOrWhiteSpace(data.phone))
            {
                var phone = _cache.Get<phoneData>("/Phone_Data_MsgSign/"+data.msgsign);
                if(phone==null)
                    return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("号码不存在", lang));
                user = _userService.GetUserByPhone(phone.phone, phone.phonearea);
            }
            else
            {
                var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
                user = _userService.GetUserByPhone(data.phone, phonearea);
            }
            if(user==null)
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
    }
}