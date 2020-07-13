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
    public class SellController : BaseApiController
    {
        private ChangerService _changer;
        private UserService _user;
        private LangService _lang;
        private PaymentService _payment;
        private IConfiguration _config;
        private PrepayService _prepayService;
        private AssetService _assetService;
        private CommonService _commonService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="changer"></param>
        /// <param name="user"></param>
        /// <param name="lang"></param>
        /// <param name="payment"></param>
        /// <param name="config"></param>
        /// <param name="prepayService"></param>
        /// <param name="assetService"></param>
        /// <param name="commonService"></param>
        public SellController(ChangerService changer, UserService user, LangService lang,
            PaymentService payment, IConfiguration config,PrepayService prepayService,AssetService assetService,CommonService commonService)
        {
            _changer = changer;
            _user = user;
            _lang = lang;
            _payment = payment;
            _config = config;
            _prepayService = prepayService;
            _assetService = assetService;
            _commonService = commonService;
        }
        /// <summary>
        /// 场外出售按钮
        /// </summary>
        /// <returns></returns>
        [HttpPost("MarketSellBtn")]
        public ActionResult<RespData<marketTradeFlag>> MarketSell()
        {
            var user = _user.GetUserByUserSN(ticket.userId);
            
            var record = _changer.GetPendingMarketSellRecordByUid(user.uid);
            if(record!=null)
            {
                return geneRetData.geneRate<marketTradeFlag>(1, new marketTradeFlag
                {
                    pendingFlag = true,
                    order = new tmpOrder
                    {
                        orderId = record.prepaysn,
                        tmpType = orderType.marketBuy,
                        orderFlag = record.flag
                    }
                });
            }
            var count = _payment.GetUPayMentCount(user.uid);
            if (count <= 0)
                return geneRetData.geneRate<marketTradeFlag>(1, new marketTradeFlag
                {
                    paymentFlag = false,
                    pendingFlag = false
                });
            return geneRetData.geneRate<marketTradeFlag>(1, new marketTradeFlag
            {
                paymentFlag=true,
                pendingFlag =  false,
                order = record == null ? null : new tmpOrder
                {
                    orderId = record.prepaysn,
                    tmpType = orderType.marketBuy,
                    orderFlag = record.flag
                }
            });
        }
        /// <summary>
        /// 场外出售界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("MarketSellView")]
        public ActionResult<RespData<marketSellView>> MarketSellView()
        {
            //var unit = "USD";
            var integral = _assetService.GetIntegralMain();
            var unit = integral.c_unit;
            var config = _assetService.GetConfigByKey("rate");
            var rate = config.d_value;
            var currency = _assetService.GetCurrencyByMarket();
            var currencySymbol = currency == null ? "¥" : currency.symbol;
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);
            var gift = _user.GetTotalUserGift(userbalance.uid);
            gift = gift * 0.1;
            var balance = userbalance.balance_reward - userbalance.frozen_reward-gift;
            var max = integral.usersell_max;
            var min = integral.usersell_min;
            return geneRetData.geneRate<marketSellView>(1, new marketSellView
            {
                unit = unit,
                title = _lang.GetLangByTitle("场外出售", lang),
                countTips = $"{min.ToString("N2")}{unit}-{max.ToString("N2")}{unit}",
                maxCount = max,
                price = rate,
                minCount = min,
                currencyCode = currency == null ? "CNY" : currency.code,
                symbol = currencySymbol,
                feeRatio = 0.005,
                balance = _commonService.CutAmount(balance, 4)
            });
        }
        /// <summary>
        /// 场外出售提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("MarketSellSubmit")]
        public ActionResult<RespData<respPrepayData>> MarketSellSubmit(marketSellData data)
        {
            var currency = _assetService.GetCurrencyByCode(data.currencyCode);
            var user = _user.GetUserByUserSN(ticket.userId);
            var rate = _assetService.GetRateByCurrencyCode(data.currencyCode.ToUpper());
            //int cid = 2;//扩展方法获取兑换商id
            if (currency == null)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("找不到货币类型", lang));
            var pending = _changer.GetPendingMarketSellRecordByUid(user.uid);
            if(pending!=null)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("您有未完成订单", lang));

            var userbalance = _user.GetBalanceByUserSN(user.guid);

            var gift = _user.GetTotalUserGift(userbalance.uid);
            gift = gift * 0.1;
            var balance = userbalance.balance_reward - userbalance.frozen_reward-gift;
            if(data.count>balance)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("余额不足", lang));
            var record = _changer.GetPendingMarketSellRecordByUid(user.uid);
            if(record!=null)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("找不到单据", lang));
            //var guid = Guid.NewGuid().ToString().Replace("-", "");
            var psn = _prepayService.InsertPrepayAsset(ticket.userId, Lion.Entity.prepayType.MarketSell,
                data.count, rate: rate.rate, currencyid: currency.id);
            if (string.IsNullOrWhiteSpace(psn))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("创建不成功"));
            return geneRetData.geneRate<respPrepayData>(1, new respPrepayData
            {
                prepay_id = psn,
                p_type = (int)prepayType.MarketSell,
                verify = 2
            });
            ////缺少给用户锁定额度
            //var ret = _changer.InsertMarketSellRecord(user.uid, user.guid, data.count, currency);
            //return geneRetData.geneRate<tmpOrder>(1, new tmpOrder
            //{
            //    orderId = ret,
            //    tmpType = orderType.marketSell,
            //    orderFlag = (int)orderFlagType.请收款
            //});
        }
        ///// <summary>
        ///// 场外出售界面
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("MarketSellConfirmView")]
        //public ActionResult<RespData<marketSellViewModel>> MarketSellConfirmView(tmpOrder data)
        //{
        //    var unit = "test";
        //    var record = _changer.GetMarketSellRecordByOrderId(data.orderId);
        //    if (record == null)
        //        return geneRetData.geneRate<marketSellViewModel>(1401, null,
        //            _lang.GetLangByTitle("找不到单据", lang));
        //    var user = _user.GetUserByUserSN(ticket.userId);
        //    if (user.uid != record.uid)//单据uid是不是匹配
        //        return geneRetData.geneRate<marketSellViewModel>(1401, null,
        //            _lang.GetLangByTitle("找不到单据", lang));
        //    var rate = 6.7;
        //    var currency = _changer.GetCurrencyById(record.currencyid);
        //    //var cpayment = _payment.GetCPayMentListByCid(record.cid);//兑换商支付方式
        //    //var paymentlist = new List<payment>();
        //    var changer = _changer.GetChangerByCid(record.cid);//获取兑换商
        //    //foreach (var item in cpayment)//添加支付方式
        //    //{
        //    //    paymentlist.Add(new payment
        //    //    {
        //    //        accountNum = item.account,
        //    //        payee = item.payee,
        //    //        payTitle = lang == "cn" ? item.pay_cnname : item.pay_name,
        //    //        qrCode = item.qrcode,
        //    //        payName = item.pay_name,
        //    //        bank = item.bank,
        //    //        payType = item.pay_type
        //    //    });
        //    //}
        //    var currencySymbol = currency == null ? "¥" : currency.symbol;
        //    long expiressTime = 0;
        //    int appealStatus = 0;
        //    int expireMinute = _config.GetValue<int>("OrderConfig:expireMinute");
        //    var now = DateTime.Now.GetTimeUnixLocal();

        //    if (data.orderFlag == (int)orderFlagType.请收款)
        //        expiressTime = record.cpaytime > 0 ? record.cpaytime + expireMinute * 60 - now : record.times + expireMinute * 60 - now;
        //    else if (data.orderFlag == (int)orderFlagType.出售订单申诉)
        //        appealStatus = 0;
        //    else if (data.orderFlag == (int)orderFlagType.出售标记完成)
        //        appealStatus = 1;
        //    else if (data.orderFlag == (int)orderFlagType.出售标记取消)
        //        appealStatus = 2;
        //    else
        //        expiressTime = 0;
        //    var amount = record.camount + record.fee;
        //    return geneRetData.geneRate<marketSellViewModel>(1, new marketSellViewModel
        //    {
        //        orderFlag = data.orderFlag,
        //        title = _lang.GetLangByTitle("场外出售", lang),
        //        amount = amount.ToString("f2"),
        //        orderno = record.orderno,
        //        unit = unit,
        //        symbol = currencySymbol,
        //        count = record.amount.ToString("f3"),
        //        price = $"{rate.ToString("f2")}",
        //        buyerPhone = changer.phone,
        //        expiressTime = expiressTime,
        //        appealStatus = appealStatus,
        //        detailTips = _lang.GetLangByTitle("详细说明", lang),
        //        payerName="test"
        //    });
        //}
        ///// <summary>
        ///// 点击已收款按钮
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("MarketSellReceiveBtn")]
        //public ActionResult<RespData<respTmpOrder>> MarketSellReceiveBtn(tmpOrderNo data)
        //{
        //    var user = _user.GetUserByUserSN(ticket.userId);
        //    //var upaytime = DateTime.Now.GetTimeUnixLocal();
        //    //int expireMinute = _config.GetValue<int>("OrderConfig:expireMinute");
        //    //var expireTime = upaytime + expireMinute * 60 - upaytime;
        //    var count = _changer.MarketSellUConfirm(data.orderId, 1);
        //    if (count > 0)
        //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
        //        {
        //            tmpType = orderType.marketSell,
        //            orderId = data.orderId,
        //            orderFlag = 1,
        //            expireTime = 0
        //        });
        //    else
        //        return geneRetData.geneRate<respTmpOrder>(1401, null, "操作不成功");
        //}
        ///// <summary>
        ///// 订单申诉按钮
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //[HttpPost("OrderAppealBtn")]
        //public ActionResult<RespData<respTmpOrder>> OrderAppealBtn(tmpOrderNo data)
        //{
        //    var user = _user.GetUserByUserSN(ticket.userId);
        //    int count = _changer.UpMarketSellFlag(data.orderId,42);
        //    if (count > 0)
        //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
        //        {
        //            orderId = data.orderId,
        //            orderFlag = 42,
        //            tmpType = orderType.marketSell
        //        });
        //    else
        //        return geneRetData.geneRate<respTmpOrder>(1401, null, "操作不成功");
        //}
        ///// <summary>
        ///// 标记已完成按钮
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("OrderConfirmSignBtn")]
        //public ActionResult<RespData<respTmpOrder>> OrderConfirmSignBtn(tmpOrderNo data)
        //{
        //    var user = _user.GetUserByUserSN(ticket.userId);
        //    //需要判断是否点击了标记按钮
        //    var record = _changer.GetMarketSellRecordByOrderId(data.orderId);
        //    if (record.flag >= (int)orderFlagType.出售标记完成)
        //        return geneRetData.geneRate<respTmpOrder>(1401, null,
        //            _lang.GetLangByTitle("不能重复标记", lang));
        //    int count = _changer.UpMarketSellFlag(data.orderId,43);
        //    if (count > 0)
        //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
        //        {
        //            orderFlag = 43,
        //            orderId = data.orderId,
        //            tmpType = orderType.marketBuy
        //        });
        //    else
        //        return geneRetData.geneRate<respTmpOrder>(1401, null, "操作不成功");
        //}
        ///// <summary>
        ///// 标记已取消按钮
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("OrderCancelSignBtn")]
        //public ActionResult<RespData<respTmpOrder>> OrderCancelSignBtn(tmpOrderNo data)
        //{
        //    var user = _user.GetUserByUserSN(ticket.userId);
        //    var record = _changer.GetMarketSellRecordByOrderId(data.orderId);
        //    if (record.flag >= (int)orderFlagType.出售标记完成)
        //        return geneRetData.geneRate<respTmpOrder>(1401, null,
        //            _lang.GetLangByTitle("不能重复标记", lang));
        //    int count = _changer.UpMarketSellFlag(data.orderId,(int)orderFlagType.出售标记取消);
        //    if (count > 0)
        //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
        //        {
        //            orderId = data.orderId,
        //            orderFlag = (int)orderFlagType.出售标记取消,
        //            tmpType = orderType.marketSell
        //        });
        //    else
        //        return geneRetData.geneRate<respTmpOrder>(1, null, "操作不成功");
        //}
        ///// <summary>
        ///// 订单询问
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("PushOrder")]
        //public ActionResult<RespData<inquiryStatus>> PushOrder(tmpOrderNo data)
        //{
        //    return geneRetData.geneRate<inquiryStatus>(1, new inquiryStatus
        //    {
        //        orderstatus = orderStatus.pending
        //    });
        //}
    }
}