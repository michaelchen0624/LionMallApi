using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.Entity;
using Lion.Services;
using Lion.ViewModel;
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
    public class VerifyController : BaseApiController
    {
        private PrepayService _prepayService;
        private LangService _langService;
        private AssetService _assetService;
        private UserService _userService;
        private ICacheService _cache;
        private CommonService _commonService;
        private IConfiguration _config;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prepayService"></param>
        /// <param name="langService"></param>
        /// <param name="assetService"></param>
        /// <param name="userService"></param>
        /// <param name="cache"></param>
        /// <param name="commonService"></param>
        /// <param name="config"></param>
        public VerifyController(PrepayService prepayService,LangService langService,
            AssetService assetService,UserService userService,ICacheService cache,
            CommonService commonService,IConfiguration config)
        {
            _prepayService = prepayService;
            _langService = langService;
            _assetService = assetService;
            _userService = userService;
            _cache = cache;
            _commonService = commonService;
            _config = config;
        }       
        /// <summary>
        /// 密码验证界面
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("PwdVerifyView")]
        public ActionResult<RespData<pwdVerifyViewModel>> PwdVerifyView(reqPayVerify data)
        {
            var ptype = (prepayType)data.p_type;
            var prepayOrder= _prepayService.GetPrepayByPsn(data.prepay_id, ptype);
            if (prepayOrder == null)
                return geneRetData.geneRate<pwdVerifyViewModel>(1401, null,
                    _langService.GetLangByTitle("找不到单据", lang));
            var asset = _assetService.GetAssetById(prepayOrder.assetId);
            var unit = asset.c_unit;
            var model = new pwdVerifyViewModel()
            {
                title = _langService.GetLangByTitle("密码验证", lang),
                prepay_id = data.prepay_id,
                p_type = data.p_type,
                symbol = ptype == prepayType.PlatBuy ? "Gcoin" : unit,
                amount = prepayOrder.amount.ToString("F4")
            };
            var list= _prepayService.GetTitleContentList(prepayOrder,ptype,lang,unit);
            model.subItems = list;
            return geneRetData.geneRate<pwdVerifyViewModel>(1, model);
        }
        /// <summary>
        /// 密码验证提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("PwdVerifySubmit")]
        public ActionResult<RespData<bool>> PwdVerifySubmit(reqPayVerify data)
        {
            if (string.IsNullOrWhiteSpace(data.pwd))
                return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("密码不能为空", lang));
            var pwd = SecureHelper.AESEncrypt(data.pwd, "lion");
            var user = _userService.GetUserByUsnAndPaypwd(ticket.userId, pwd);
            if (user == null)
                return geneRetData.geneRate<bool>(1, false);
            var ptype = (prepayType)data.p_type;
            if (data.verify == 1)//只有密码验证
            {
                var ret= _prepayService.HandlePrepayOrder(data.prepay_id, ptype);
                if(ret!="success")
                    return geneRetData.geneRate<bool>(1401, false,
                    _langService.GetLangByTitle("保存不成功", lang));
            }
            else
            {
                _cache.Set(CacheKey.PayVerify + data.prepay_id, 2, 300, false);
            }
            return geneRetData.geneRate<bool>(1, true);
        }
        /// <summary>
        /// 短信验证界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("MsgVerifyView")]
        public ActionResult<RespData<msgVerifyViewModel>> MsgVerifyView(reqPayVerify data)
        {
            int step = _cache.Get<int>(CacheKey.PayVerify + data.prepay_id);
            if (step != 2)//第一不验证不通过
                return geneRetData.geneRate<msgVerifyViewModel>(1, new msgVerifyViewModel
                {
                    step = step == 0 ? 1 : step
                });
            var user = _userService.GetUserByUserSN(ticket.userId);
            if (user == null)
                throw new LionException("用户不存在", 404);
            var ret = _commonService.SendMsg(user.phone, 6, msgType.Verify, user.phonearea);
            if (ret.Contains("errmsg"))
            {
                var msg = ret.Split(":");
                throw new LionException(_langService.GetLangByTitle($"{msg[msg.Length - 1]}", lang),
                    404);
            }
            return geneRetData.geneRate<msgVerifyViewModel>(1, new msgVerifyViewModel
            {
                expireSecond = 120,
                msgsign = ret
            });
        }
        /// <summary>
        /// 短信验证提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("MsgVerifySubmit")]
        public ActionResult<RespData<msgVerifyRet>> MsgVerifySubmit(reqPayVerify data)
        {
            if(string.IsNullOrWhiteSpace(data.msgcode))
                return geneRetData.geneRate<msgVerifyRet>(1401, null,
                    _langService.GetLangByTitle("验证码不能为空", lang));
            int step = _cache.Get<int>(CacheKey.PayVerify + data.prepay_id);
            if (step != 2)//第一不验证不通过
                return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                {
                    step = step == 0 ? 1 : step,
                    ret = false
                });
            var flag = _commonService.verifyMsg(data.msgsign, data.msgcode,6);
            if(flag)
            {
                if(data.verify==2)
                {
                    var ptype = (prepayType)data.p_type;
                    var ret = _prepayService.HandlePrepayOrder(data.prepay_id, ptype);
                    if (ptype == prepayType.MarketSell)
                    {
                        if(ret=="error")
                            return geneRetData.geneRate<msgVerifyRet>(1401, null,
                            _langService.GetLangByTitle("保存不成功", lang));
                        else
                            return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                            {
                                ret = true,
                                order = new tmpOrder
                                {
                                    orderId = ret,
                                    orderFlag = (int)orderFlagType.请收款,
                                    tmpType = orderType.marketSell
                                }
                            });
                    }
                    else
                    {
                        if (ret != "success")
                            return geneRetData.geneRate<msgVerifyRet>(1401, null,
                            _langService.GetLangByTitle("保存不成功", lang));
                        else
                            return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                            {
                                ret = true,
                            });
                    }
                }
                return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                {
                    ret = true
                });
                //return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                //{                    
                //    ret = true,
                //    order=(prepayType)data.p_type==prepayType.MarketSell?new tmpOrder {
                //        orderId=data.prepay_id,orderFlag= (int)orderFlagType.请收款,
                //        tmpType= orderType.marketSell
                //    } :
                //    null
                //});
            }
            else
            {
                var code = _cache.Get<string>(data.msgsign);
                if (string.IsNullOrWhiteSpace(code))
                    return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                    {
                        expire=-1,
                        ret=false
                    });
                return geneRetData.geneRate<msgVerifyRet>(1, new msgVerifyRet
                {
                    ret = false
                });
            }
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
            var ret = _commonService.SendMsg(user.phone, 6, msgType.Verify, user.phonearea, expireSecond: expire);
            if (ret.Contains("errmsg"))
            {
                var msg = ret.Split(":");
                throw new LionException(_langService.GetLangByTitle($"{msg[msg.Length - 1]}", lang),
                    404);
            }
            return geneRetData.geneRate<msgSign>(1, new msgSign { sign = ret, expire = expire > 120 ? 120 : expire });
        }
    }
}