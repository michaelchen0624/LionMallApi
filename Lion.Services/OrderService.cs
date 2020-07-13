using DataCommon;
using Lion.Entity;
using Lion.ViewModel;
using Lion.ViewModel.requestModel;
using Newtonsoft.Json;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lion.Services
{
    public class OrderService
    {
        private ChangerService _changerService;
        private AssetService _assetService;
        private ILogService _log;
        private UserService _userService;
        private RpcNotifyService _rpcService;
        private CommonService _commonService;
        public OrderService(ChangerService changerService,AssetService assetService,ILogService log,
            UserService userService,RpcNotifyService rpcService,CommonService commonService)
        {
            _changerService = changerService;
            _assetService = assetService;
            _log = log;
            _userService = userService;
            _rpcService = rpcService;
            _commonService = commonService;
        }
        public int HandleChainTest(NotifyPayData data)
        {
            var ret = _commonService.SendMsg("18556575866", 4, msgType.Register, "86", phonekey: false);
            if (ret.Contains("errmsg"))
            {
                _log.E("Test", $"18556575866短信发送失败:{ret}");
            }
            var ret2 = _commonService.SendMsg("15333583099", 4, msgType.Register, "86", phonekey: false);
            if (ret2.Contains("errmsg"))
            {
                _log.E("Test", $"15333583099短信发送失败:{ret}");
            }
            //var ret3 = _commonService.SendMsg("1120829653", 4, msgType.Register, "60", phonekey: false);
            //if (ret3.Contains("errmsg"))
            //{
            //    _log.E("Test", $"1120829653短信发送失败:{ret}");
            //}

            return 1;
        }

        public int HandleChainOrder(NotifyPayData data)
        {
            int t_type = (int)rechargeType.General;
            var recharge = DataService.Get<rechargerecordEntity>(o => o.txid == data.txid && o.recharge_type == t_type);
            if (recharge != null)
                return 0;
            var asset_name = data.asset.ToUpper();
            var integral = _assetService.GetAssetByCName(asset_name);
            if(integral==null)
            {
                _log.E("chainPurchase", $"不可知coin类型,{JsonConvert.SerializeObject(data)}");
                return 0;
            }
            double c_amount = 0;
            var flag=double.TryParse(data.amount, out c_amount);
            if (!flag)
            {
                _log.E("chainPurchase", $"amount转换错误,{JsonConvert.SerializeObject(data)}");
                return 0;
            }
            var amount = c_amount / integral.ratio;
            var recharseaddr = _assetService.GetRechargeAddrByAddress(data.address);
            if(recharseaddr==null)
            {
                _log.E("chainPurchase", $"地址不存在,{JsonConvert.SerializeObject(data)}");
                return 0;
            }
            var nowdate = DateTime.Now;
            var now = nowdate.GetTimeUnixLocal();
            DataService.Transcation(tc =>
            {
                int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == recharseaddr.uid).Incre(o => new userbalanceEntity
                {
                    balance_shopping = amount,
                    recharge_total = amount
                }).UpdateOnlyIncre();
                int count2 = tc.CommandSet<rechargerecordEntity>().Insert(new rechargerecordEntity
                {
                    amount = amount,
                    asset_code = 1,
                    uid = recharseaddr.uid,
                    times = now,
                    batch = nowdate.ToString("yyyy-MM-dd"),
                    recharge_type = (int)rechargeType.General,
                    c_name=integral.c_name,
                    c_amount=c_amount,
                    txid=data.txid
                });
                int count3 = tc.CommandSet<userextendEntity>().Where(o => o.uid == recharseaddr.uid).Incre(o => new userextendEntity
                {
                    total_consum = amount
                }).UpdateOnlyIncre();
                if (count1 <= 0 || count1 <= 0 || count2 <= 0 || count3 <= 0)
                    throw new PayException("handle_error", 404);
            });
            Task.Run(() =>
            {
                _userService.LevelCalculate(recharseaddr.uid);
            });
            var currency= _assetService.GetCurrencyByCode("CNY");
            var exchangerrate = _assetService.GetRateByCurrencyCode("CNY");
            var camount = exchangerrate.rate * amount;
            Task.Run(() =>
            {
                _rpcService.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                {
                    usn = recharseaddr.usn,
                    amount = amount,
                    currency = currency.id,
                    p_type = LionPType.普通充值,
                    rate = exchangerrate.rate,
                    camount = camount,
                    //fee = fee,
                    o_time = now,
                    //fee=record.fee,
                    //out_trade_no = record.orderno
                });
            });
            return 1;
        }
        public int HandleChangerOrder(string ordersn,string usn,int orderType,string status,long h_time,
            double camount = 0, double fee = 0, double rate = 0,int initiator=0)
        {
            switch(orderType)
            {
                case 1://用户买单
                    try
                    {
                        UserBuy(ordersn, usn, status, h_time,camount,fee,rate,initiator);
                    }
                    catch(Exception ex)
                    {
                        _log.E("Handle", ex.ToString());
                    }
                    break;
                case 2://用户出售
                    try
                    {
                        UserSell(ordersn, usn, status, h_time,camount,fee,rate);
                    }
                    catch(Exception ex)
                    {
                        _log.E("Handle", ex.ToString());
                    }
                    break;
                default:
                    break;
            }
            return 0;
        }
        private int UserBuy(string ordersn,string usn,string status,long h_time,double camount=0,double fee=0,double rate=0,int initiator=0)
        {
            var record = _changerService.GetMarketBuyByOrderId(ordersn);
            if (record.flag == -1 || record.flag == 1)
                return 0;
            var nowdate = DateTime.Now;
            var now = nowdate.GetTimeUnixLocal();
            
            if (status.Contains(orderNotifyStatus.success.ToString()))//订单已完成
            {
                var asset = _assetService.GetIntegralMain();
                var amount = record.amount;               
                DataService.Transcation(tc =>
                {
                    int count= tc.CommandSet<marketbuyrecordEntity>().Where(o => o.orderid == ordersn)
                    .Update(o => new marketbuyrecordEntity
                    {
                        flag=1,
                        confirmtime=h_time,
                        camount=camount,
                        fee=fee,
                        rate=rate
                    });
                    int count1=tc.CommandSet<userbalanceEntity>().Where(o => o.uid == record.uid).Incre(o => new userbalanceEntity
                    {
                        balance_shopping=amount,
                        recharge_total=amount
                    }).UpdateOnlyIncre();
                    int count2=tc.CommandSet<rechargerecordEntity>().Insert(new rechargerecordEntity
                    {
                        amount=amount,
                        asset_code=asset.id,
                        uid=record.uid,
                        times=now,
                        batch=nowdate.ToString("yyyy-MM-dd"),
                        recharge_type= (int)rechargeType.Market,
                        out_trade_no=ordersn
                    });
                    int count3 = tc.CommandSet<userextendEntity>().Where(o => o.uid == record.uid).Incre(o => new userextendEntity
                    {
                        total_consum=amount
                    }).UpdateOnlyIncre();
                    if (count <= 0 || count1 <= 0 || count2 <= 0 || count3<=0)
                        throw new PayException("handle_error", 404);
                });
                Task.Run(() =>
                {
                    _userService.LevelCalculate(record.uid);
                });
                Task.Run(() =>
                {
                    _rpcService.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                    {
                        usn=usn,
                        amount=amount,
                        currency=record.currencyid,
                        p_type=LionPType.场外充值,
                        rate=rate,
                        camount=camount,
                        fee=fee,
                        o_time=h_time,
                        //fee=record.fee,
                        out_trade_no=record.orderno
                    });
                });
            }
            else if(status.Contains(orderNotifyStatus.cancel.ToString()))//订单已取消
            {
                //var cancelcount = _changerService.CancelCountOfCurrent(record.uid);
                //增加用户取消计数
                DataService.Transcation(tc =>
                {
                    int count= tc.CommandSet<marketbuyrecordEntity>().Where(o => o.orderid == ordersn)
                    .Update(o => new marketbuyrecordEntity
                    {
                        flag = -1,
                        confirmtime = h_time,
                        camount=camount,
                        fee=fee,
                        rate=rate
                    });
                    int count1=tc.CommandSet<cancelrecordEntity>().Insert(new cancelrecordEntity
                    {
                        ordersn=ordersn,
                        c_time=h_time,
                        times=now,
                        uid=record.uid,
                        usn=usn
                    });
                    int count2 = tc.CommandSet<usercancelEntity>().Insert(new usercancelEntity
                    {
                        ordersn=ordersn,
                        uid=record.uid,
                        usn=usn,
                        times=h_time
                    });
                    if (count <= 0 || count1 <= 0 || count2<=0)
                        throw new PayException("handle_error", 404);
                });
            }
            return 1;
        }
        private int UserSell(string ordersn,string usn,string status,long h_time,double camount = 0, double fee = 0, double rate = 0)
        {
            var record = _changerService.GetMarketSellByOrderId(ordersn);
            if (record.flag == -1 || record.flag == 1)
                return 0;
            var amount = record.amount;
            if (status.Contains(orderNotifyStatus.success.ToString()))//订单已完成
            {
                var asset = _assetService.GetIntegralMain();
                DataService.Transcation(tc =>
                {
                    int count = tc.CommandSet<marketsellrecordEntity>().Where(o => o.orderid == ordersn)
                    .Update(o => new marketsellrecordEntity
                    {
                        flag = 1,
                        confirmtime = h_time,
                        camount=camount,
                        fee=fee,
                        rate=rate
                    });
                    int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == record.uid).Incre(o => new userbalanceEntity
                    {
                        balance_reward = -amount,
                        frozen_reward=-amount
                    }).UpdateOnlyIncre();
                    if (count <= 0 || count1 <= 0)
                        throw new PayException("handle_error", 404);
                });
                Task.Run(() =>
                {
                    _rpcService.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                    {
                        usn = usn,
                        amount = amount,
                        currency = record.currencyid,
                        p_type = LionPType.场外提现,
                        rate = rate,
                        camount = camount,
                        o_time = h_time,
                        fee=fee,
                        out_trade_no = record.orderno
                    });
                });
            }
            else if (status.Contains(orderNotifyStatus.cancel.ToString()))//订单已取消
            {
                var asset = _assetService.GetIntegralMain();
                DataService.Transcation(tc =>
                {
                    int count = tc.CommandSet<marketsellrecordEntity>().Where(o => o.orderid == ordersn)
                    .Update(o => new marketsellrecordEntity
                    {
                        flag = -1,
                        confirmtime = h_time,
                        camount=camount,
                        fee=fee,
                        rate=rate
                    });
                    int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == record.uid).Incre(o => new userbalanceEntity
                    {
                        //balance_reward = -amount,
                        frozen_reward = -amount
                    }).UpdateOnlyIncre();
                    if (count <= 0 || count1 <= 0)
                        throw new PayException("handle_error", 404);
                });
            }
            return 1;
        }
    }
}
