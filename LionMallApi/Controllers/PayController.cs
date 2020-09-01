using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lion.Entity;
using Lion.Services;
using Lion.ViewModel;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Prod.Core;
using LionMall.Tools;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PayController : ControllerBase
    {
        private PrepayService _prepayService;
        private AssetService _assetService;
        private LangService _lang;
        private UserService _userService;
        private RpcNotifyService _rpcService;
        private ILogService _log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prepayService"></param>
        /// <param name="lang"></param>
        /// <param name="assetService"></param>
        /// <param name="userService"></param>
        /// <param name="rpcService"></param>
        /// <param name="log"></param>
        public PayController(PrepayService prepayService, LangService lang,
            AssetService assetService, UserService userService, RpcNotifyService rpcService, ILogService log)
        {
            _prepayService = prepayService;
            _lang = lang;
            _assetService = assetService;
            _userService = userService;
            _rpcService = rpcService;
            _log = log;
        }
        /// <summary>
        /// 下单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("unifiedorder")]
        public ActionResult<RespData<respPrepay>> unifiedorder(reqUnifiedOrder data)
        {
            _log.I("unifiedorder", JsonConvert.SerializeObject(data));
            var paydata = new PayData<reqUnifiedOrder>(data);
            var prepayid = string.Empty;
            if (paydata.CheckSign(PaySignType.MD5, "1234"))//验签不通过抛出
            {
                var user = _userService.GetUserByUserSN(data.usn);
                if (user == null)
                    return geneRetData.geneRate<respPrepay>(1401, null,
                    _lang.GetLangByTitle("user does not exist", "cn"));
                if (string.IsNullOrWhiteSpace(user.paypwd))
                    return geneRetData.geneRate<respPrepay>(806, null,
                    _lang.GetLangByTitle("nopaypasswd", "cn"));
                if (data.total_amount <= 0)
                    return geneRetData.geneRate<respPrepay>(1401, null,
                    _lang.GetLangByTitle("Incorrect amount format", "cn"));
                var amount = Math.Round(data.total_amount / 100.00, 2);
                var currency = _assetService.GetCurrencyByCode(data.currency.ToUpper());
                prepayid = _prepayService.InsertPrepayOrder(data.order_no, data.usn, amount, data.product, currency);
            }
            else
            {
                return geneRetData.geneRate<respPrepay>(1401, null,
                    _lang.GetLangByTitle("signerror", "cn"));
            }
            if (string.IsNullOrWhiteSpace(prepayid))
                return geneRetData.geneRate<respPrepay>(1401, null,
                    _lang.GetLangByTitle("Unsuccessful", "cn"));
            var resp = new respPrepay
            {
                mch_id = data.mch_id,
                order_no = data.order_no,
                nonce_str = data.nonce_str,
                user_sn = data.usn,
                prepay_id = prepayid,
                p_type = (int)prepayType.Shopping,
                verify = 1
            };
            var retdata = new PayData<respPrepay>(resp);
            var sign = retdata.MakeSign(PaySignType.MD5, "1234");
            resp.sign = sign;
            return geneRetData.geneRate<respPrepay>(1, resp);
        }
        /// <summary>
        /// 商品退货
        /// </summary>
        /// <returns></returns>
        [HttpPost("Ret")]
        public ActionResult<RespData<respGoodsCanc>> ProductRet(reqGoodsOrder data)
        {
            var model = _prepayService.GoodsCanc(data);
            return geneRetData.geneRate<respGoodsCanc>(1,model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("NewUser")]
        public ActionResult<RespData<respNewUser>> NewUser(reqDate data)
        {
            _log.I("getTotal", $"NewUser:{JsonConvert.SerializeObject(data)}");
            var paydata = new PayData<reqDate>(data);
            paydata.CheckSign(PaySignType.MD5, "1234");
            DateTime from;
            DateTime to;
            var flag= DateTime.TryParse(data.from, out from);
            var flag2 = DateTime.TryParse(data.to, out to);
            if (!flag || !flag2)
                return geneRetData.geneRate<respNewUser>(1401, null, "日期格式不对");
            from = new DateTime(from.Year, from.Month, from.Day);
            to = new DateTime(to.Year, to.Month, to.Day);
            var result = _userService.GetNewUserTotal(from, to.AddDays(1).AddSeconds(-1));
            return geneRetData.geneRate<respNewUser>(1, result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("NewPurchase")]
        public ActionResult<RespData<string>> NewPurchase(reqDate data)
        {
            var paydata = new PayData<reqDate>(data);
            paydata.CheckSign(PaySignType.MD5, "1234");
            return geneRetData.geneRate<string>(1, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("NewIssue")]
        public ActionResult<RespData<respNewIssue>> NewIssue(reqDate data)
        {
            _log.I("getTotal", $"NewIssue:{JsonConvert.SerializeObject(data)}");
            var paydata = new PayData<reqDate>(data);
            paydata.CheckSign(PaySignType.MD5, "1234");
            DateTime from;
            DateTime to;
            var flag = DateTime.TryParse(data.from, out from);
            var flag2 = DateTime.TryParse(data.to, out to);
            if (!flag || !flag2)
                return geneRetData.geneRate<respNewIssue>(1401, null, "日期格式不对");
            var result= _userService.GetNewIssueTotal(from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));
            return geneRetData.geneRate<respNewIssue>(1, result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("Gift")]
        public ActionResult<RespData<bool>> Gift(GiftData data)
        {
            var paydata = new PayData<GiftData>(data);
            paydata.CheckSign(PaySignType.MD5, "1234");
            var phonearea = StringHelper.GetNumberFromStr(data.phoneArea);
            var user = _userService.GetUserByPhone(data.phone, phonearea);
            if (user == null)
                return geneRetData.geneRate<bool>(404, false,"电话号码不存在");
            double amount = 0;
            var flag= double.TryParse(data.amount, out amount);
            if (!flag)
                return geneRetData.geneRate<bool>(1401, false, "金额格式不正确");
            var count= _userService.SetGift(user, amount);
            return geneRetData.geneRate<bool>(1, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("GList")]
        public ActionResult<RespData<userGift>> GList(reqDate data)
        {
            var paydata = new PayData<reqDate>(data);
            paydata.CheckSign(PaySignType.MD5, "1234");
            DateTime from;
            DateTime to;
            var flag = DateTime.TryParse(data.from, out from);
            var flag2 = DateTime.TryParse(data.to, out to);
            if (!flag || !flag2)
                return geneRetData.geneRate<userGift>(1401,null, "日期格式不对");
            from = new DateTime(from.Year, from.Month, from.Day);
            to = new DateTime(to.Year, to.Month, to.Day);
            var result = _userService.GetUserGift(from, to.AddDays(1).AddSeconds(-1));
            return geneRetData.geneRate<userGift>(1, result);
        }
    }
}