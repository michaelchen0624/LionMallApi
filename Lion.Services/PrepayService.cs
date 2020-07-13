using DataCommon;
using Lion.Entity;
using Lion.ViewModel;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lion.Services
{
    /// <summary>
    /// 预付单号
    /// </summary>
    public class PrepayService
    {
        private AssetService _assetService;
        private LangService _langService;
        private UserService _userService;
        private RpcNotifyService _rpcnotify;
        private IConfiguration _config;
        private ChangerService _changerService;
        private ILogService _log;
        public PrepayService(AssetService assetService,LangService lang,ChangerService changerService,
            UserService userService,RpcNotifyService rpcnotify,IConfiguration config,ILogService log)
        {
            _assetService = assetService;
            _langService = lang;
            _changerService = changerService;
            _userService = userService;
            _rpcnotify = rpcnotify;
            _config = config;
            _log = log;
        }
        public prepayorderEntity GetPrepayOrderByPsn(string psn)
        {
            return DataService.Get<prepayorderEntity>(o => o.psn == psn);
        }
        public prepayassetEntity GetPrepayAssetByPsn(string psn)
        {
            return DataService.Get<prepayassetEntity>(o => o.psn == psn);
        }
        /// <summary>
        /// 生成资产预付单
        /// </summary>
        /// <returns></returns>
        public string InsertPrepayAsset(string usn,prepayType ptype, double amount,
            double fee=0,double rate=0,int currencyid=0,string receiveAddr="",string receiveSn="", string orderno = "")
        {
            var psn = Guid.NewGuid().ToString().Replace("-", "");
            var entity = new prepayassetEntity
            {
                out_trade_no = orderno,
                usn = usn,
                asset_code = 1,
                psn = psn,
                p_type = (int)ptype,
                times = DateTime.Now.GetTimeUnixLocal(),
                amount = amount,
                fee=fee,
                receive_sn=receiveSn,
                receive_addr=receiveAddr,
                rate=rate,
                currency_id=currencyid
            };
            var count = DataService.Insert<prepayassetEntity>(entity);
            if (count > 0)
                return psn;
            else
                return string.Empty;
        }
        public string InsertPrepayOrder(string out_trade_no,string usn,double camount,string product,currencyEntity currency)
        {
            if (currency == null)
                throw new LionException("Incorrect currency type", 1401);
            var rate = _assetService.GetRateByCurrencyCode(currency.code);
            var amount = Math.Round(camount / rate.rate, 4);
            var psn = Guid.NewGuid().ToString().Replace("-", "");
            var entity = new prepayorderEntity
            {
                out_trade_no=out_trade_no,
                //order_no=orderno,
                usn=usn,
                asset_code=rate.asset_code,
                psn=psn,
                currency_id=currency.id,
                p_type=(int)prepayType.Shopping,
                times=DateTime.Now.GetTimeUnixLocal(),
                amount=amount,
                camount=camount,
                rate=rate.rate,
                extend=product
            };
            var count= DataService.Insert<prepayorderEntity>(entity);
            if (count > 0)
                return psn;
            else
                return string.Empty;
        }
        public prepayRet GetPrepayByPsn(string psn, prepayType type)
        {
            var ret = new prepayRet();
            if(type.Equals(prepayType.Shopping))
            {
                var shoporder = DataService.Get<prepayorderEntity>(o => o.psn == psn);
                if (shoporder != null)
                {
                    ret.flag = shoporder.flag == 0 ? 1 : shoporder.flag;
                    ret.amount = shoporder.amount;
                    ret.camount = shoporder.camount;
                    ret.currencyId = shoporder.currency_id;
                    ret.rate = shoporder.rate;
                    ret.assetId = shoporder.asset_code;
                }
            }
            else
            {
                var shoporder = DataService.Get<prepayassetEntity>(o => o.psn == psn);
                if (shoporder != null)
                {
                    ret.flag = shoporder.flag == 0 ? 1 : shoporder.flag;
                    ret.amount = shoporder.amount;
                    ret.assetId = shoporder.asset_code;
                    ret.fee = shoporder.fee;
                    ret.rate = shoporder.rate;
                    ret.currencyId = shoporder.currency_id;
                    ret.receiveSn = shoporder.receive_sn;
                }
            }
            return ret;
        }
        public IList<titleContent> GetTitleContentList(prepayRet data,prepayType ptype,string lang,
            string assetUnit="us")
        {
            var list = new List<titleContent>();
            var currency = data.currencyId == 0 ? null : _assetService.GetCurrencyById(data.currencyId);
            switch(ptype)
            {
                case prepayType.Shopping:
                    {
                        //var currency = _assetService.GetCurrencyById(data.currencyId);
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("订单金额", lang),
                            content = $"{currency.symbol}{data.camount.ToString("F2")}"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("参考汇率", lang),
                            content = $"1{currency.code}={Math.Round(1 / data.rate, 3)}{assetUnit}"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("详情", lang),
                            content = "购物消费"
                        });
                        break;
                    }
                case prepayType.Withdrawal:
                    {
                        //var asset = _assetService.GetAssetById(data.assetId);
                        //var unit = asset.c_unit;
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("手续费", lang),
                            content = $"{data.fee.ToString("F2")}{assetUnit}"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("到账数量", lang),
                            content = $"{(data.amount-data.fee).ToString("F3")}{assetUnit}"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("详情", lang),
                            content = _langService.GetLangByTitle("普通提现", lang)
                        });
                        break;
                    }
                case prepayType.MarketSell:
                    {
                        var camount = Math.Round(data.rate * data.amount, 2);
                        var fee = Math.Round(camount * 0.005, 2);
                        //var currency = _assetService.GetCurrencyById(data.currencyId);
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("手续费", lang),
                            content = $"{fee.ToString("F2")}{currency.code}"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("到账金额", lang),
                            content = $"{(camount - fee).ToString("F2")}{currency.code}"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("详情", lang),
                            content = _langService.GetLangByTitle("场外出售", lang)
                        });
                        break;
                    }
                case prepayType.Transfer:
                    {
                        //var recevice = _userService.GetUserByUserSN(data.receiveSn);
                        //list.Add(new titleContent
                        //{
                        //    title = _langService.GetLangByTitle("接收者电话", lang),
                        //    content = ""
                        //});
                        //list.Add(new titleContent
                        //{
                        //    title = _langService.GetLangByTitle("到账金额", lang),
                        //    content = $"{(camount - fee).ToString("F2")}{currency.code}"
                        //});
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("详情", lang),
                            content = _langService.GetLangByTitle("站内互转", lang)
                        });
                        break;
                    }
                case prepayType.Reward:
                    {
                        //list.Add(new titleContent
                        //{
                        //    title = _langService.GetLangByTitle("手续费", lang),
                        //    content = $"{fee.ToString("F2")}{currency.code}"
                        //});
                        //list.Add(new titleContent
                        //{
                        //    title = _langService.GetLangByTitle("到账金额", lang),
                        //    content = $"{(camount - fee).ToString("F2")}{currency.code}"
                        //});
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("详情", lang),
                            content = _langService.GetLangByTitle("站内充值", lang)
                        });
                        break;
                    }
                case prepayType.PlatBuy:
                    {
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("回购价", lang),
                            content = $"{data.rate.ToString("N3")} {assetUnit}/Gcoin"
                        });
                        list.Add(new titleContent
                        {
                            title = _langService.GetLangByTitle("详情", lang),
                            content = _langService.GetLangByTitle("均价回购", lang)
                        });
                        break;
                    }
                default:break;
            }
            return list;
        }
        /// <summary>
        /// 处理预付单
        /// </summary>
        /// <returns></returns>
        public string HandlePrepayOrder(string psn,prepayType ptype)
        {
            var ret = string.Empty;
            switch(ptype)
            {
                case prepayType.Shopping:
                    ret=HandleShopping(psn);
                    break;
                case prepayType.MarketSell:
                    ret=HandleMarketSell(psn);
                    break;
                case prepayType.Withdrawal:
                    ret=HandleWithdrawal(psn);
                    break;
                case prepayType.Transfer:
                    ret=HandleTransfer(psn);
                    break;
                case prepayType.Reward:
                    ret = HandleRewardToShopping(psn);
                    break;
                case prepayType.PlatBuy:
                    ret = HandlePlatBuy(psn);
                    break;
                default:
                    break;
            }
            return ret;
        }
        /// <summary>
        /// 处理购物
        /// </summary>
        /// <returns></returns>
        private string HandleShopping(string psn)
        {
            var prepay = GetPrepayOrderByPsn(psn);
            if (prepay == null)
                throw new LionException("找不到单号", 1401);
            var user = _userService.GetUserByUserSN(prepay.usn);
            var now = DateTime.Now.GetTimeUnixLocal();
            var pay_no = _userService.GetUserOrderNumber();
            DataService.Transcation(tc =>
            {
                int count = tc.CommandSet<shoprecordEntity>().Insert(new shoprecordEntity
                {
                    psn = prepay.psn,
                    currency_id = prepay.currency_id,
                    amount = prepay.amount,
                    camount = prepay.camount,
                    asset_code = prepay.asset_code,
                    out_trade_no = prepay.out_trade_no,
                    pay_no = pay_no,
                    rate = prepay.rate,
                    times = now,
                    uid = user.uid,
                    usn = user.guid
                });
                int count1= tc.CommandSet<userbalanceEntity>().Where(o => o.uid == user.uid).
                Incre(o => new userbalanceEntity
                {
                    balance_shopping=-prepay.amount
                }).UpdateOnlyIncre();
                if (count <= 0 || count1 <= 0)
                    throw new LionException("处理失败",1401);
            });
            var respnotify = new respNotiry
            {
                order_no = prepay.psn,
                out_trade_no = prepay.out_trade_no,
                camount = prepay.camount,
                mch_id="200222",
                rate=prepay.rate
            };
            var paydata = new PayData<respNotiry>(respnotify);
            var sign= paydata.MakeSign(PaySignType.MD5, "1234");
            respnotify.sign = sign;
            var url = _config.GetValue<string>("OrderConfig:notifyUrl");
            Task.Run(() =>
            {
                _rpcnotify.SendShopping(url, JsonConvert.SerializeObject(respnotify));
            });
            Task.Run(() =>
            {
                _rpcnotify.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                {
                    usn = prepay.usn,
                    amount = prepay.amount,
                    currency = 1,
                    p_type = LionPType.购物消费,
                    rate = prepay.rate,
                    camount = prepay.camount,
                    o_time = now,
                    out_trade_no = prepay.out_trade_no,
                    order_no=pay_no,
                    extend=prepay.extend
                });
            });
            return "success";
        }
        /// <summary>
        /// 处理普通提现
        /// </summary>
        /// <returns></returns>
        private string HandleWithdrawal(string psn)
        {
            var prepay = GetPrepayAssetByPsn(psn);
            if (prepay == null)
                throw new LionException("找不到单号", 1401);
            var user = _userService.GetUserByUserSN(prepay.usn);
            DataService.Transcation(tc =>
            {
                int count = tc.CommandSet<withdrawEntity>().Insert(new withdrawEntity
                {
                    usn=user.guid,
                    asset_id=prepay.asset_code,
                    amount=prepay.amount,
                    fee=prepay.fee,
                    guid=prepay.psn,
                    receive_addr=prepay.receive_addr
                });
                int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == user.uid).
                Incre(o => new userbalanceEntity
                {
                    frozen_reward = prepay.amount
                }).UpdateOnlyIncre();
                if (count <= 0 || count1 <= 0)
                    throw new LionException("处理失败", 1401);
            });
            return "success";
        }
        /// <summary>
        /// 处理场外出售
        /// </summary>
        /// <returns></returns>
        private string HandleMarketSell(string psn)
        {
            var prepay = GetPrepayAssetByPsn(psn);
            if (prepay == null)
                throw new LionException("找不到单号", 1401);
            var user = _userService.GetUserByUserSN(prepay.usn);
            var currency = _assetService.GetCurrencyById(prepay.currency_id);
            var phone = $"{user.phonearea}{user.phone}";
            var ret = _changerService.InsertMarketSellRecord(user.uid, user.guid,phone, prepay.amount, currency);
            //var camount = Math.Round(prepay.rate * prepay.amount, 2);
            //var fee = Math.Round(camount * 0.005, 2);
            //DataService.Transcation(tc =>
            //{
            //    var count = tc.CommandSet<marketsellrecordEntity>().Insert(new Lion.Entity.marketsellrecordEntity
            //    {
            //        uid = user.uid,
            //        orderid = prepay.psn,
            //        orderno = LionRandom.GenerateRandomCode(10),
            //        amount = prepay.amount,
            //        rate = prepay.rate,
            //        camount = camount,
            //        fee = fee,
            //        currencyid =prepay.currency_id,
            //        cid = 2,
            //        flag = (int)orderFlagType.请收款,//请收款状态
            //        times = DateTime.Now.GetTimeUnixLocal()
            //    });
            //    int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == user.uid).
            //    Incre(o => new userbalanceEntity
            //    {
            //        frozen_shopping = prepay.amount
            //    }).UpdateOnlyIncre();
            //    if (count <= 0 || count1 <= 0)
            //        throw new LionException("处理失败", 1401);
            //});
            if (string.IsNullOrWhiteSpace(ret))
                return "error";
            return ret;
        }
        /// <summary>
        /// 处理站内互转
        /// </summary>
        /// <returns></returns>
        private string HandleTransfer(string psn)
        {
            var prepay = GetPrepayAssetByPsn(psn);
            if (prepay == null)
                throw new LionException("找不到单号", 1401);
            var user = _userService.GetUserByUserSN(prepay.usn);
            var receive = _userService.GetUserByUserSN(prepay.receive_sn);
            var nowdate = DateTime.Now;
            var now = nowdate.GetTimeUnixLocal();
            DataService.Transcation(tc =>
            {
                var count = tc.CommandSet<transferEntity>().Insert(new Lion.Entity.transferEntity
                {
                    sender=user.guid,
                    sender_id=user.uid,
                    receive=receive.guid,
                    receive_id=receive.uid,
                    amount=prepay.amount,
                    times = now
                });
                int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == user.uid).
                Incre(o => new userbalanceEntity
                {
                    balance_reward = -prepay.amount
                }).UpdateOnlyIncre();
                int count2 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == receive.uid).
                Incre(o => new userbalanceEntity
                {
                    balance_shopping = prepay.amount,//转到购物者的购物余额
                    recharge_total=prepay.amount
                }).UpdateOnlyIncre();
                int count3= tc.CommandSet<userextendEntity>().Where(o => o.uid == receive.uid).Incre(o => new userextendEntity
                {
                    total_consum = prepay.amount
                }).UpdateOnlyIncre();
                int count4 = tc.CommandSet<rechargerecordEntity>().Insert(new rechargerecordEntity
                {
                    amount = prepay.amount,
                    asset_code = 1,
                    uid = receive.uid,
                    times = now,
                    batch=nowdate.ToString("yyyy-MM-dd"),
                    recharge_type = (int)rechargeType.Transfer,
                    sender=user.uid
                    //out_trade_no = ordersn
                });
                if (count <= 0 || count1 <= 0 || count2 <= 0 || count3 <= 0 || count4 <= 0)
                    throw new LionException("处理失败", 1401);
            });
            Task.Run(() =>
            {
                _userService.LevelCalculate(receive.uid);
            });
            return "success";
        }
        /// <summary>
        /// 处理奖励余额转购物余额
        /// </summary>
        /// <param name="psn"></param>
        /// <returns></returns>
        private string HandleRewardToShopping(string psn)
        {
            var prepay = GetPrepayAssetByPsn(psn);
            if (prepay == null)
                throw new LionException("找不到单号", 1401);
            var user = _userService.GetUserByUserSN(prepay.usn);
            var amount = prepay.amount;
            var nowdate = DateTime.Now;
            var now = nowdate.GetTimeUnixLocal();
            DataService.Transcation(tc =>
            {
                var count = tc.CommandSet<rechargeinternalEntity>().Insert(new Lion.Entity.rechargeinternalEntity
                {
                    uid=user.uid,
                    usn=user.guid,
                    amount=amount,
                    times = now
                });
                int count1 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == user.uid).
                Incre(o => new userbalanceEntity
                {
                    balance_reward = -amount,
                    balance_shopping=amount,
                    recharge_total=amount
                }).UpdateOnlyIncre();
                int count2 = tc.CommandSet<userextendEntity>().Where(o => o.uid == user.uid).Incre(o => new userextendEntity
                {
                    total_consum = prepay.amount
                }).UpdateOnlyIncre();
                int count3 = tc.CommandSet<rechargerecordEntity>().Insert(new rechargerecordEntity
                {
                    amount = amount,
                    asset_code = 1,
                    uid = user.uid,
                    times = now,
                    batch = nowdate.ToString("yyyy-MM-dd"),
                    recharge_type = (int)rechargeType.Reward,
                    usn=user.guid
                    //out_trade_no = ordersn
                });
                if (count <= 0 || count1 <= 0 || count2 <= 0 || count3 <= 0)
                    throw new LionException("处理失败", 1401);
            });
            Task.Run(() =>
            {
                _userService.LevelCalculate(user.uid);
            });
            Task.Run(() =>
            {
                _rpcnotify.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                {
                    usn = prepay.usn,
                    amount = prepay.amount,
                    currency = 1,
                    p_type = LionPType.站内充值,
                    rate = prepay.rate,
                    o_time = now,
                    out_trade_no = prepay.out_trade_no
                });
            });
            return "success";
        }

        /// <summary>
        /// 处理平台回购
        /// </summary>
        /// <returns></returns>
        private string HandlePlatBuy(string psn)
        {
            var prepay = GetPrepayAssetByPsn(psn);
            if (prepay == null)
                throw new LionException("找不到单号", 1401);
            var user = _userService.GetUserByUserSN(prepay.usn);
            var amount = prepay.amount;
            var price = prepay.rate;//平台回购价格 0.1 usdt/gcoin
            var u_balance = Math.Round(amount * price, 4);
            var now = DateTime.Now.GetTimeUnixLocal();
            DataService.Transcation(tc =>
            {
                int count = tc.CommandSet<walletEntity>().Where(o => o.uid == user.uid).Incre(o => new walletEntity
                {
                    balance = -amount
                }).UpdateOnlyIncre();
                int count2 = tc.CommandSet<userbalanceEntity>().Where(o => o.uid == user.uid).Incre(o => new userbalanceEntity
                {
                    balance_reward = u_balance
                }).UpdateOnlyIncre();
                int count1 = tc.CommandSet<platbuyrecordEntity>().Insert(new platbuyrecordEntity
                {
                    uid=user.uid,
                    b_type=2,
                    amount=amount,
                    price=price,
                    conver_us=u_balance,
                    times=now
                });
                if (count <= 0 || count2 <= 0 || count1<=0)
                    throw new LionException("处理失败", 500);
            });
            Task.Run(() =>
            {
                _rpcnotify.FlowOrderData(new ViewModel.requestModel.reqOrderFlowData
                {
                    usn = prepay.usn,
                    amount = u_balance,
                    currency = 1,
                    p_type = LionPType.均价回购,
                    rate = price,
                    camount = amount,
                    o_time = now
                });
            });
            return "success";
        }


        public respGoodsCanc GoodsCanc(reqGoodsOrder data)
        {
            var paydata = new PayData<reqGoodsOrder>(data);
            _log.I("GoodsCanc", JsonConvert.SerializeObject(data));
            var flag= paydata.CheckSign(PaySignType.MD5, "1234");
            if (!flag)
                throw new PayException("验签不正确", 1401);
            var shoprecord = DataService.Get<shoprecordEntity>(o => o.psn == data.psn);
            if (shoprecord == null)
                throw new PayException("找不到记录", 1401);
            var camount = Math.Round(data.camount / 100.00,2);
            var amount = Math.Round(camount / shoprecord.rate, 4);
            var gsn = Guid.NewGuid().ToString().Replace("-", "");
            var productName = data.productName.Length > 50 ? data.productName.Substring(0, 50) : data.productName;
            var now = DateTime.Now.GetTimeUnixLocal();
            DataService.Transcation(tc =>
            {
                int count = tc.CommandSet<goodscancEntity>().Insert(new goodscancEntity
                {
                    psn = data.psn,
                    usn = data.usn,
                    amount = amount,
                    camount = camount,
                    rate = shoprecord.rate,
                    pro_name =productName ,
                    gsn = gsn,
                    times = now
                });
                int count2=tc.CommandSet<userbalanceEntity>().Where(o => o.usn == data.usn).Incre(o => new userbalanceEntity
                {
                    balance_shopping=amount
                }).UpdateOnlyIncre();
                if (count <= 0 || count2 <= 0)
                    throw new PayException("保存失败", 1401);
            });
            Task.Run(() =>
            {
                _rpcnotify.FlowOrderData(new reqOrderFlowData
                {
                    out_trade_no=data.psn,
                    amount=amount,
                    camount=camount,
                    currency=1,
                    o_time=now,
                    plat=1,
                    p_type=LionPType.购物退款,
                    rate=shoprecord.rate,
                    usn=data.usn,
                    extend=productName
                });
            });
            return new respGoodsCanc
            {
                order_sn=gsn
            };
        }
    }
}
