using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.Services;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using LionMall.Tools;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : BaseApiController
    {
        private ChangerService _changer;
        private UserService _user;
        private LangService _lang;
        private PaymentService _payment;
        private IConfiguration _config;
        private AssetService _assetService;
        private ILogService _log;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="changer"></param>
        /// <param name="user"></param>
        /// <param name="lang"></param>
        /// <param name="payment"></param>
        /// <param name="config"></param>
        /// <param name="assetService"></param>
        /// <param name="log"></param>
        public PurchaseController(ChangerService changer,UserService user,LangService lang,
            PaymentService payment,IConfiguration config,AssetService assetService,ILogService log)
        {
            _changer = changer;
            _user = user;
            _lang = lang;
            _payment = payment;
            _config = config;
            _assetService = assetService;
            _log = log;
        }
        /// <summary>
        /// 点击购买按钮
        /// </summary>
        /// <returns></returns>
        [HttpPost("MarketBuy")]
        public ActionResult<RespData<marketTradeFlag>> MarketBuy()
        {
            var user = _user.GetUserByUserSN(ticket.userId);
            var record = _changer.GetPendingMarketBuyRecordByUid(user.uid);

            return geneRetData.geneRate<marketTradeFlag>(1, new marketTradeFlag
            {
                pendingFlag = record == null ? false : true,
                order = record == null ? null : new tmpOrder
                {
                    orderFlag=1,
                    orderId=record.prepaysn,
                    tmpType=orderType.marketBuy
                }
            });
        }
        /// <summary>
        /// 获取链上充值类型
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositList")]
        public ActionResult<RespData<depoistView>> GetDepositList()
        {
            var dlist = _assetService.GetIntegralListByCoinType(2);
            var list = new List<depositTypeItem>();
            var wlist = new List<depositTypeItem>();
            foreach(var item in dlist)
            {
                list.Add(new depositTypeItem
                {
                    id=item.id,
                    c_name=item.c_name,
                    coinType=item.coin_type
                });
                if (item.c_name.ToLower() == "usdt")
                    wlist.Add(new depositTypeItem
                    {
                        id=item.id,
                        c_name=item.c_name,
                        coinType=item.coin_type
                    });
            }
            var view = new depoistView()
            {
                purchase=list,
                withdrawal=wlist
            };
            return geneRetData.geneRate<depoistView>(1,view);
        }
        /// <summary>
        /// 充值界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("RechargeView")]
        public ActionResult<RespData<depositView>> Recharge(CoinTypeData data)
        {
            //_log.I("test", data.id.ToString());
            //var balance = _user.GetBalanceByUserSN(ticket.userId);
            var addr = _user.GetRechargeAddrByUsnAndCoinType(ticket.userId, data.id);
            //int assetCode = 1;
            //Lion.Entity.rechargeaddrEntity addr = null;
            //if (addr == null)
            //{
            //    var user = _user.GetUserByUserSN(ticket.userId);
            //    var integraladdr = _assetService.GetIntegralAddr(ticket.userId, 1);
            //    addr = new Lion.Entity.rechargeaddrEntity
            //    {
            //        addr=integraladdr.addr,
            //        public_key=integraladdr.key,
            //        asset_code=assetCode,
            //        usn=ticket.userId,
            //        uid=user.uid
            //    };
            //    var count= _assetService.InsertRechargeAddr(addr);
            //    if(count<=0)
            //    {
            //        addr = _assetService.GetRechargeAddrByUsnAndAsset(ticket.userId, 1);
            //        if(addr==null)
            //        _log.E("rechargeAddr", $"usn:{ticket.userId},addr:{integraladdr.addr},assetcode:{assetCode},获取数据为空");
            //    }                    
            //}
            var chainType = new string[] { "ERC20" };
            return geneRetData.geneRate<depositView>(1, new depositView
            {
                address = addr,
                title = _lang.GetLangByTitle("充值", lang),
                chainType=chainType,
                tips = _lang.GetLangByTitle("充值提示", lang)
            });
        }
        /// <summary>
        /// C2C购买界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("MarketBuyView")]
        public ActionResult<RespData<marketBuyView>> MarketBuyView()
        {
            var asset = _assetService.GetIntegralMain();
            var unit = asset.c_unit;
            var config = _assetService.GetConfigByKey("rate");
            var rate = config.d_value;
            var currency = _assetService.GetCurrencyByMarket();
            var currencySymbol = currency == null ? "¥" : currency.symbol;
            var userlevel = _user.GetUserExtendByUsn(ticket.userId);
            var levelconfig = _user.GetLevelConfigList().Where(o=>o.id==userlevel.level).FirstOrDefault();
            var max = levelconfig == null ? 500 : levelconfig.userbuy_max;
            var min = levelconfig == null ? 10 : levelconfig.userbuy_min;
            return geneRetData.geneRate<marketBuyView>(1, new marketBuyView
            {
                unit = unit,
                title = _lang.GetLangByTitle("场外购买", lang),
                countTips = $"{min.ToString("N2")}{unit}-{max.ToString("N2")}{unit}",
                //maxAmount = 5000,
                maxCount = max,
                price = rate,
                minCount = min,
                //pricestr = $"{currencySymbol}6.7",
                //cancelSecond = 30,
                currencyCode = currency == null ? "CNY" : currency.code,
                symbol = currencySymbol,
                feeRatio = 0.00
            });
        }
        /// <summary>
        /// C2C购买提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("MarketBuySubmit")]
        public ActionResult<RespData<tmpOrder>> MarketBuySubmit(marketBuyData data)
        {
            var currency = _assetService.GetCurrencyByCode(data.currencyCode);
            var user = _user.GetUserByUserSN(ticket.userId);

                        //var camount = Math.Round((rate * data.count) * (1 + 0.005), 2);
            //var camount = Math.Round(rate * data.count, 2);
            //var fee = Math.Round(camount * 0.005, 2);
            if (currency == null)
                return geneRetData.geneRate<tmpOrder>(1401, null,
                    _lang.GetLangByTitle("找不到货币类型", lang));

            var record = _changer.GetPendingMarketBuyRecordByUid(user.uid);
            if(record!=null)
                return geneRetData.geneRate<tmpOrder>(1401, null,
                    _lang.GetLangByTitle("您有未完成订单", lang));

            var cancelcount = _changer.CancelCountOfCurrent(user.uid);
            if (cancelcount >= 3)
                return geneRetData.geneRate<tmpOrder>(1401, null,
                    _lang.GetLangByTitle("购买订单已取消超过3单,请次日下单", lang));
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var phone = $"{user.phonearea}{user.phone}";
            var ret = _changer.InsertMarketBuyRecord(user.uid,user.guid,phone ,data.count, currency);
            //var count = _changer.InsertMarketBuyRecord(new Lion.Entity.marketbuyrecordEntity
            //{
            //    uid = user.uid,
            //    orderid = guid,
            //    orderno = LionRandom.GenerateRandomCode(10),
            //    amount = data.count,
            //    rate = 6.7,
            //    camount = camount,
            //    fee = fee,
            //    currencyid = currency.id,
            //    cid = cid,
            //    flag = 21,//请付款状态
            //    times = DateTime.Now.GetTimeUnixLocal()
            //});
            //缺少给兑换商锁定额度
            return geneRetData.geneRate<tmpOrder>(1, new tmpOrder
            {
                orderId = ret,
                tmpType = orderType.marketBuy,
                orderFlag = (int)orderFlagType.请付款
            });
        }
            ///// <summary>
            ///// C2C购买提交
            ///// </summary>
            ///// <returns></returns>
            //[HttpPost("MarketBuySubmit")]
            //public ActionResult<RespData<tmpOrder>> MarketBuySubmit(marketBuyData data)
            //{
            //    var currency = _changer.GetCurrencyByCode(data.currencyCode);
            //    var user = _user.GetUserByUserSN(ticket.userId);
            //    var rate = 6.7;
            //    int cid = 2;//扩展方法获取兑换商id
            //    //var camount = Math.Round((rate * data.count) * (1 + 0.005), 2);
            //    var camount = Math.Round(rate * data.count, 2);
            //    var fee = Math.Round(camount * 0.005, 2);
            //    if (currency == null)
            //        return geneRetData.geneRate<tmpOrder>(1401, null,
            //            _lang.GetLangByTitle("找不到货币类型", lang));
            //    var guid = Guid.NewGuid().ToString().Replace("-", "");
            //    var count = _changer.InsertMarketBuyRecord(new Lion.Entity.marketbuyrecordEntity
            //    {
            //        uid = user.uid,
            //        orderid = guid,
            //        orderno=LionRandom.GenerateRandomCode(10),
            //        amount = data.count,
            //        rate=6.7,
            //        camount = camount,
            //        fee=fee,
            //        currencyid = currency.id,
            //        cid = cid,
            //        flag=21,//请付款状态
            //        times = DateTime.Now.GetTimeUnixLocal()
            //    });
            //    //缺少给兑换商锁定额度
            //    return geneRetData.geneRate<tmpOrder>(1, new tmpOrder
            //    {
            //        orderId = guid,
            //        tmpType = orderType.marketBuy,
            //        orderFlag=21
            //    });
            //}
            ///// <summary>
            ///// 场外购买界面
            ///// </summary>
            ///// <returns></returns>
            //[HttpPost("MarketBuyConfirmView")]
            //public ActionResult<RespData<marketBuyViewModel>> MarketBuyConfirmView(tmpOrder data)
            //{
            //    var unit = "test";
            //    var record = _changer.GetMarketRecordByOrderId(data.orderId);
            //    if (record == null)
            //        return geneRetData.geneRate<marketBuyViewModel>(1401, null,
            //            _lang.GetLangByTitle("找不到单据", lang));
            //    var user = _user.GetUserByUserSN(ticket.userId);
            //    if (user.uid != record.uid)//单据uid是不是匹配
            //        return geneRetData.geneRate<marketBuyViewModel>(1401, null,
            //            _lang.GetLangByTitle("找不到单据", lang));
            //    var rate = 6.7;
            //    var currency = _changer.GetCurrencyById(record.currencyid);
            //    var cpayment = _payment.GetCPayMentListByCid(record.cid);//兑换商支付方式
            //    var paymentlist = new List<payment>();
            //    var changer = _changer.GetChangerByCid(record.cid);//获取兑换商
            //    foreach (var item in cpayment)//添加支付方式
            //    {
            //        paymentlist.Add(new payment
            //        {
            //            accountNum = item.account,
            //            payee = item.payee,
            //            payTitle = lang == "cn" ? item.pay_cnname : item.pay_name,
            //            qrCode = item.qrcode,
            //            payName = item.pay_name,
            //            bank=item.bank,
            //            payType=item.pay_type
            //        });
            //    }
            //    var currencySymbol = currency == null ? "¥" : currency.symbol;
            //    long expiressTime = 0;
            //    int appealStatus = 0;
            //    int expireMinute = _config.GetValue<int>("OrderConfig:expireMinute");
            //    if (data.orderFlag == 21)
            //        expiressTime = record.times + expireMinute * 60 - DateTime.Now.GetTimeUnixLocal();
            //    else if (data.orderFlag == 22)
            //        expiressTime = record.upaytime + expireMinute * 60 - DateTime.Now.GetTimeUnixLocal();
            //    else if (data.orderFlag == 24)
            //        appealStatus = 1;
            //    else if (data.orderFlag == 25)
            //        appealStatus = 2;
            //    else
            //        expiressTime = 0;
            //    //var amount = record.camount + record.fee;
            //    var amount = record.camount;
            //    return geneRetData.geneRate<marketBuyViewModel>(1, new marketBuyViewModel
            //    {
            //        orderFlag=data.orderFlag,
            //        title = _lang.GetLangByTitle("场外购买", lang),
            //        amount = amount.ToString("f2"),
            //        orderno=record.orderno,
            //        unit = unit,
            //        symbol = currencySymbol,
            //        count = record.amount.ToString("f3"),
            //        paymentList = paymentlist,
            //        price = $"{rate.ToString("f2")}",
            //        sellerPhone = changer.phone,
            //        expiressTime = expiressTime,
            //        appealStatus=appealStatus,
            //        detailTips = _lang.GetLangByTitle("详细说明", lang)
            //    });
            //}
            ///// <summary>
            ///// 点击已付款按钮
            ///// </summary>
            ///// <returns></returns>
            //[HttpPost("MarketBuyPayBtn")]
            //public ActionResult<RespData<respTmpOrder>> MarketBuyPayBtn(tmpOrderNo data)
            //{
            //    var user = _user.GetUserByUserSN(ticket.userId);
            //    var upaytime = DateTime.Now.GetTimeUnixLocal();
            //    int expireMinute = _config.GetValue<int>("OrderConfig:expireMinute");
            //    var expireTime = upaytime + expireMinute * 60 - upaytime;
            //    var count= _changer.UpMarketRecordUPayTime(data.orderId,user.uid,upaytime);
            //    if (count > 0)
            //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
            //        {
            //            tmpType=orderType.marketBuy,
            //            orderId=data.orderId,
            //            orderFlag=22,
            //            expireTime=expireTime
            //        });
            //    else
            //        return geneRetData.geneRate<respTmpOrder>(1401, null, "操作不成功");
            //}
            ///// <summary>
            ///// 点击取消按钮
            ///// </summary>
            ///// <returns></returns>
            //[HttpPost("MarketBuyCancelBtn")]
            //public ActionResult<RespData<respTmpOrder>> MarketBuyCancelBtn(tmpOrderNo data)
            //{
            //    var user = _user.GetUserByUserSN(ticket.userId);
            //    int count = _changer.UpMarketRecordFlag(data.orderId,user.uid, -1);
            //    //缺少兑换商解开额度,取消次数加1
            //    if (count > 0)
            //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
            //        {
            //            orderId=data.orderId,
            //            tmpType=orderType.marketBuy
            //        });
            //    else
            //        return geneRetData.geneRate<respTmpOrder>(1401,null, "操作不成功");
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
            //    int count = _changer.UpMarketRecordFlag(data.orderId, user.uid, 23);
            //    if (count > 0)
            //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
            //        {
            //            orderId=data.orderId,
            //            orderFlag=23,
            //            tmpType=orderType.marketBuy
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
            //    var record = _changer.GetMarketRecordByOrderId(data.orderId);
            //    if(record.flag>=24)
            //        return geneRetData.geneRate<respTmpOrder>(1401, null,
            //            _lang.GetLangByTitle("不能重复标记", lang));
            //    int count = _changer.UpMarketRecordFlag(data.orderId, user.uid, 24);
            //    if (count > 0)
            //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
            //        {
            //            orderFlag=24,
            //            orderId=data.orderId,
            //            tmpType=orderType.marketBuy
            //        });
            //    else
            //        return geneRetData.geneRate<respTmpOrder>(1401,null, "操作不成功");
            //}
            ///// <summary>
            ///// 标记已取消按钮
            ///// </summary>
            ///// <returns></returns>
            //[HttpPost("OrderCancelSignBtn")]
            //public ActionResult<RespData<respTmpOrder>> OrderCancelSignBtn(tmpOrderNo data)
            //{
            //    var user = _user.GetUserByUserSN(ticket.userId);
            //    int count = _changer.UpMarketRecordFlag(data.orderId, user.uid, 25);
            //    if (count > 0)
            //        return geneRetData.geneRate<respTmpOrder>(1, new respTmpOrder
            //        {
            //            orderId=data.orderId,
            //            orderFlag=25,
            //            tmpType=orderType.marketBuy
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
            //        orderstatus=orderStatus.pending
            //    });
            //}
        }
}