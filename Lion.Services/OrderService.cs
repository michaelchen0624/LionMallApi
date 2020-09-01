using DataCommon;
using Lion.Entity;
using Lion.ViewModel;
using Lion.ViewModel.requestModel;
using Newtonsoft.Json;
using Prod.Core;
using System;
using System.Threading.Tasks;
using LionMall.Tools;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using Google.Protobuf.WellKnownTypes;

namespace Lion.Services
{
    public class OrderService
    {
        private IConfiguration _config;
        private IHttpUtils _httpUtils;
        private ChangerService _changerService;
        private AssetService _assetService;
        private ILogService _log;
        private UserService _userService;
        private RpcNotifyService _rpcService;
        private CommonService _commonService;
        public OrderService(ChangerService changerService, AssetService assetService, ILogService log,
            UserService userService, RpcNotifyService rpcService, CommonService commonService, IConfiguration config, IHttpUtils utils)
        {
            _changerService = changerService;
            _assetService = assetService;
            _log = log;
            _userService = userService;
            _rpcService = rpcService;
            _commonService = commonService;
            _config = config;
            _httpUtils = utils;
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
            if (integral == null)
            {
                _log.E("chainPurchase", $"不可知coin类型,{JsonConvert.SerializeObject(data)}");
                return 0;
            }
            double c_amount = 0;
            var flag = double.TryParse(data.amount, out c_amount);
            if (!flag)
            {
                _log.E("chainPurchase", $"amount转换错误,{JsonConvert.SerializeObject(data)}");
                return 0;
            }
            var amount = c_amount / integral.ratio;
            var recharseaddr = _assetService.GetRechargeAddrByAddress(data.address);
            if (recharseaddr == null)
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
                    c_name = integral.c_name,
                    c_amount = c_amount,
                    txid = data.txid
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
            var currency = _assetService.GetCurrencyByCode("CNY");
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
        public int HandleChangerOrder(string ordersn, string usn, int orderType, string status, long h_time,
            double camount = 0, double fee = 0, double rate = 0, int initiator = 0)
        {
            switch (orderType)
            {
                case 1://用户买单
                    try
                    {
                        UserBuy(ordersn, usn, status, h_time, camount, fee, rate, initiator);
                    }
                    catch (Exception ex)
                    {
                        _log.E("Handle", ex.ToString());
                    }
                    break;
                case 2://用户出售
                    try
                    {
                        UserSell(ordersn, usn, status, h_time, camount, fee, rate);
                    }
                    catch (Exception ex)
                    {
                        _log.E("Handle", ex.ToString());
                    }
                    break;
                default:
                    break;
            }
            return 0;
        }
        private int UserBuy(string ordersn, string usn, string status, long h_time, double camount = 0, double fee = 0, double rate = 0, int initiator = 0)
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
                    int count = tc.CommandSet<marketbuyrecordEntity>().Where(o => o.orderid == ordersn)
                    .Update(o => new marketbuyrecordEntity
                    {
                        flag = 1,
                        confirmtime = h_time,
                        camount = camount,
                        fee = fee,
                        rate = rate
                    });
                    int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == record.uid).Incre(o => new userbalanceEntity
                    {
                        balance_shopping = amount,
                        recharge_total = amount
                    }).UpdateOnlyIncre();
                    int count2 = tc.CommandSet<rechargerecordEntity>().Insert(new rechargerecordEntity
                    {
                        amount = amount,
                        asset_code = asset.id,
                        uid = record.uid,
                        times = now,
                        batch = nowdate.ToString("yyyy-MM-dd"),
                        recharge_type = (int)rechargeType.Market,
                        out_trade_no = ordersn
                    });
                    int count3 = tc.CommandSet<userextendEntity>().Where(o => o.uid == record.uid).Incre(o => new userextendEntity
                    {
                        total_consum = amount
                    }).UpdateOnlyIncre();
                    if (count <= 0 || count1 <= 0 || count2 <= 0 || count3 <= 0)
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
                        usn = usn,
                        amount = amount,
                        currency = record.currencyid,
                        p_type = LionPType.场外充值,
                        rate = rate,
                        camount = camount,
                        fee = fee,
                        o_time = h_time,
                        //fee=record.fee,
                        out_trade_no = record.orderno
                    });
                });
            }
            else if (status.Contains(orderNotifyStatus.cancel.ToString()))//订单已取消
            {
                //var cancelcount = _changerService.CancelCountOfCurrent(record.uid);
                //增加用户取消计数
                DataService.Transcation(tc =>
                {
                    int count = tc.CommandSet<marketbuyrecordEntity>().Where(o => o.orderid == ordersn)
                    .Update(o => new marketbuyrecordEntity
                    {
                        flag = -1,
                        confirmtime = h_time,
                        camount = camount,
                        fee = fee,
                        rate = rate
                    });
                    int count1 = tc.CommandSet<cancelrecordEntity>().Insert(new cancelrecordEntity
                    {
                        ordersn = ordersn,
                        c_time = h_time,
                        times = now,
                        uid = record.uid,
                        usn = usn
                    });
                    int count2 = tc.CommandSet<usercancelEntity>().Insert(new usercancelEntity
                    {
                        ordersn = ordersn,
                        uid = record.uid,
                        usn = usn,
                        times = h_time
                    });
                    if (count <= 0 || count1 <= 0 || count2 <= 0)
                        throw new PayException("handle_error", 404);
                });
            }
            return 1;
        }
        private int UserSell(string ordersn, string usn, string status, long h_time, double camount = 0, double fee = 0, double rate = 0)
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
                        camount = camount,
                        fee = fee,
                        rate = rate
                    });
                    int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == record.uid).Incre(o => new userbalanceEntity
                    {
                        balance_reward = -amount,
                        frozen_reward = -amount
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
                        fee = fee,
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
                        camount = camount,
                        fee = fee,
                        rate = rate
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

        //场外充值
        public int ExchangeBuyOrder(string out_trade_no)
        {
            try
            {
                var url = _config.GetValue<string>("OrderConfig:changerUrl");
                url += "?module=proxy&action=eth_getTransactionByHash&txhash=" + out_trade_no + "&apikey=BJXHID3AEKK5IH9YWP62HA9H942HZ3ATF2";
                var ret = _httpUtils.httpGet(url);
                //var ret = "{\"id\": 1,\"result\": {\"from\": \"0x258373d54427470c1dde09b0153bde0b161767d6\",\"hash\": \"0xbbb6db4c4d66393daf9a45903cdf379265562bb7e44819b34392ea64c2737ff9\",\"to\": \"0xea7eea3735592df7d6822d1c2f1a314eda66ca8e\",\"value\": \"0xde0b6b3a7640000\",\"s\": \"0x11020491c3782ae1d9529ade146f315238ff3d259fbdb5b147ff5ba52fbd4357\"}}";
                JObject jsondata = JObject.Parse(ret);
                if (jsondata.ContainsKey("error") || !jsondata.ContainsKey("result"))
                {
                    throw new PayException("error或未包含关键字result", 1401);
                }
                var result = JObject.Parse(jsondata["result"].ToString());
                if (!result.ContainsKey("hash"))
                {
                    throw new PayException("未报含关键字hash", 1401);
                }
                if (result["hash"].ToString() != out_trade_no)
                {
                    throw new PayException("订单号不一致", 1401);
                }
                string address = result["to"].ToString();
                var recharseaddr = _assetService.GetRechargeAddrByAddress(address);
                if (recharseaddr == null)
                {
                    throw new PayException("未找到该用户地址", 1401);
                }

                string value = result["value"].ToString();
                var val = value.Substring(2, value.Length - 2);
                long amount = Convert.ToInt64(val, 16);
                var balance = _userService.GetBalanceByUserSN(recharseaddr.usn);

                var asset = _assetService.GetIntegralMain();
                var exchangerrate = _assetService.GetRateByCurrencyCode("CNY");
                var nowdate = DateTime.Now;
                var now = nowdate.GetTimeUnixLocal();

                var buyrecord = _changerService.GetMarketBuyByOrderNo(out_trade_no);
                var buyrecord2 = DataService.Get<marketbuyrecordEntity>(o => o.flag == 21 && o.uid == recharseaddr.uid);
                if (buyrecord != null)
                {
                    throw new PayException("已完成订单", 1401);
                }
                else if (buyrecord2 != null)
                {
                    DataService.Transcation(tc =>
                    {
                        //int count = tc.CommandSet<marketbuyrecordEntity>().Where(o => o.flag == 21 && o.uid == recharseaddr.uid)
                        //.Update(o => new marketbuyrecordEntity
                        //{
                        //    flag = 1,
                        //    confirmtime = now,
                        //    camount = exchangerrate.rate * amount,
                        //    orderno = out_trade_no,
                        ////fee = fee,
                        //rate = exchangerrate.rate
                        //});
                        buyrecord2.flag = 1;
                        buyrecord2.confirmtime = now;
                        buyrecord2.camount = exchangerrate.rate * amount;
                        buyrecord2.orderno = out_trade_no;
                        buyrecord2.rate = exchangerrate.rate;
                        int count = tc.CommandSet<marketbuyrecordEntity>().Update(buyrecord2);
                        int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == balance.uid).Incre(o => new userbalanceEntity
                        {
                            balance_shopping = amount + o.balance_shopping,
                            recharge_total = amount + o.recharge_total
                        }).UpdateOnlyIncre();
                        int count2 = tc.CommandSet<rechargerecordEntity>().Insert(new rechargerecordEntity
                        {
                            amount = amount,
                            asset_code = asset.id,
                            uid = balance.uid,
                            times = now,
                            batch = nowdate.ToString("yyyy-MM-dd"),
                            recharge_type = (int)rechargeType.Market,
                            out_trade_no = out_trade_no
                        });
                        int count3 = tc.CommandSet<userextendEntity>().Where(o => o.uid == balance.uid).Incre(o => new userextendEntity
                        {
                            total_consum = amount + o.total_consum
                        }).UpdateOnlyIncre();
                        if (count != 1 || count1 <= 0 || count2 <= 0 || count3 <= 0)
                            throw new PayException("handle_error", 404);
                    });
                    Task.Run(() =>
                    {
                        _userService.LevelCalculate(balance.uid);
                    });
                    Task.Run(() =>
                    {
                        _rpcService.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                        {
                            usn = balance.usn,
                            amount = amount,
                            currency = 1,//USDT
                            p_type = LionPType.场外充值,
                            rate = exchangerrate.rate,
                            camount = exchangerrate.rate * amount,
                            fee = 0,
                            o_time = now,
                            out_trade_no = out_trade_no
                        });
                    });
                } 
            }
            catch (Exception ex)
            {
                _log.E("Handle", ex.ToString());
            }
            return 0;
        }

        //链上提现
        public int ExchangeSellOrder(string out_trade_no)
        {
            var url = _config.GetValue<string>("OrderConfig:changerUrl");
            url += "?module=proxy&action=eth_getTransactionByHash&txhash=" + out_trade_no + "&apikey=BJXHID3AEKK5IH9YWP62HA9H942HZ3ATF2";
            var ret = _httpUtils.httpGet(url);
            //var ret = "{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":{\"blockHash\":\"0xf1e2c8adbe1e69ab92f86077d24f54abbfb5ff2ed87683a38f9c9008c02c1198\",\"from\":\"0x5a9cf70048db6f7b1c96b651a9a8c58567d73b0c\",\"hash\":\"0xf18c822657f12646c51b3ce1133a4ca71819abebc7fe59b1d9cc6e10a8020004\",\"input\":\"0xa9059cbb000000000000000000000000ef79e049d27f1d91d2ab81deab28da5f4934d8ed00000000000000000000000000000000000000000000000000000000000f424a\",\"to\":\"0xdac17f958d2ee523a2206206994597c13d831ec7\"}}";
            JObject jsondata = JObject.Parse(ret);
            if (jsondata.ContainsKey("error") || !jsondata.ContainsKey("result"))
            {
                throw new PayException("error或未包含关键字result", 1401);
            }
            var result = JObject.Parse(jsondata["result"].ToString());
            if (!result.ContainsKey("hash"))
            {
                throw new PayException("未报含关键字hash", 1401);
            }
            if (result["hash"].ToString() != out_trade_no)
            {
                throw new PayException("订单号不一致", 1401);
            }
            string input = result["input"].ToString();
            string address = "0x" + input.Substring(34, 40);
            var recharseaddr = _assetService.GetRechargeAddrByAddress(address);
            if (recharseaddr == null)
            {
                throw new PayException("未找到该用户地址", 1401);
            }

            string value = input.Substring(74);
            long amount = Convert.ToInt64(value, 16);
            
            var nowdate = DateTime.Now;
            var now = nowdate.GetTimeUnixLocal();

            var sellrecord = _changerService.GetMarketSellByOrderNo(out_trade_no);
            var sellrecord2 = DataService.Get<marketsellrecordEntity>(o => o.flag == 21 && o.uid == recharseaddr.uid);
            if (sellrecord != null)
            {
                throw new PayException("已完成订单", 1401);
            }
            else if(sellrecord2 != null)
            {
                DataService.Transcation(tc =>
                {
                    //int count = tc.CommandSet<marketsellrecordEntity>().Where(o => o.flag == 21 && o.uid == recharseaddr.uid)
                    //.Update(o => new marketsellrecordEntity
                    //{
                    //    flag = 1,
                    //    confirmtime = now,
                    //    camount = amount,
                    //    fee = o.amount - amount,
                    ////rate = rate,
                    //orderno = out_trade_no
                    //});
                    sellrecord2.flag = 1;
                    sellrecord2.confirmtime = now;
                    sellrecord2.camount = amount;
                    sellrecord2.fee = sellrecord2.amount - amount;
                    //buyrecord2.rate = 0;
                    sellrecord2.orderno = out_trade_no;
                    int count = tc.CommandSet<marketsellrecordEntity>().Update(sellrecord2);
                    int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == recharseaddr.uid).Incre(o => new userbalanceEntity
                    {
                        //balance_reward = -sellrecord2.amount,
                        frozen_reward = -sellrecord2.amount
                    }).UpdateOnlyIncre();
                    if (count != 1 || count1 <= 0)
                        throw new PayException("handle_error", 404);
                });
                Task.Run(() =>
                {
                    _rpcService.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                    {
                        usn = recharseaddr.usn,
                        amount = sellrecord2.amount,
                        currency = 1,
                        p_type = LionPType.场外提现,
                        rate = 0,
                        camount = amount,
                        o_time = now,
                        fee = sellrecord2.amount - amount,
                        out_trade_no = out_trade_no
                    });
                });
            }
            return 1;
        }
    }
}
