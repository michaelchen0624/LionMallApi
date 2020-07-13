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
using Prod.Core;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private UserService _user;
        //private ChangerService _changer;
        private LangService _lang;
        private PaymentService _payment;
        private PrepayService _prepayService;
        private AssetService _assetService;
        private CommonService _commonService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lang"></param>
        /// <param name="payment"></param>
        /// <param name="prepayService"></param>
        /// <param name="assetService"></param>
        /// <param name="commonService"></param>
        public AccountController(UserService user,LangService lang,PaymentService payment,
            PrepayService prepayService,AssetService assetService,CommonService commonService)
        {
            _user = user;
            //_changer = changer;
            _lang = lang;
            _payment = payment;
            _prepayService = prepayService;
            _assetService = assetService;
            _commonService = commonService;
        }
        /// <summary>
        /// 钱包界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("WalletView")]
        public ActionResult<RespData<walletView>> WalletView()
        {
            var balance = _user.GetBalanceByUserSN(ticket.userId);
            var integral = _assetService.GetIntegralMain();
            var totalbalance = balance.balance_shopping + balance.balance_reward
                - balance.frozen_reward - balance.frozen_shopping;
            var buybalance = balance.balance_shopping - balance.frozen_shopping;
            var rewardbalance = balance.balance_reward - balance.frozen_reward;
            return geneRetData.geneRate<walletView>(1, new walletView
            {
                unit = integral.c_unit,
                //buyBalance=Math.Round(buybalance,4),
                //rewardBalance=Math.Round(rewardbalance,4),
                //totalBalance=Math.Round(totalbalance,4)
                buyBalance = buybalance.ToString("N4"),
                rewardBalance = rewardbalance.ToString("N4"),
                totalBalance = totalbalance.ToString("N4")
            });
        }
        /// <summary>
        /// 获取用户余额
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUserBalance")]
        public ActionResult<RespData<userBalance>> GetUserBalance()
        {
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);
            var balance = userbalance.balance_shopping - userbalance.frozen_shopping;
            var config = _assetService.GetConfigByKey("rate");
            var rate = config.d_value;
            return geneRetData.geneRate<userBalance>(1, new userBalance
            {
                balance = Math.Round(balance, 4),
                currency_balance = Math.Round(balance * rate, 2)
            });
        }
        /// <summary>
        /// 链上提现界面
        /// </summary>
        [HttpPost("ChainWithDrawView")]
        public ActionResult<RespData<chainWithDrawView>> ChainWithDrawView(CoinTypeData data)
        {
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);
            var asset = _assetService.GetAssetById(data.id);
            var user = _user.GetUserByUsnNotCache(userbalance.usn);

            var gift = _user.GetTotalUserGift(userbalance.uid);
            gift = gift * 0.1;

            var balance = userbalance.balance_reward - userbalance.frozen_reward - gift;
            return geneRetData.geneRate<chainWithDrawView>(1, new chainWithDrawView
            {
                title = $"提币{asset.c_unit}",
                //unit = asset.c_unit,
                unit = asset.c_unit,
                chainType=new string[] {"ERC20"},
                fee = asset.withdraw_fee,
                min = asset.withdraw_min,
                balance = _commonService.CutAmount(balance,5),
                detailTips = "",
            });
        }
        /// <summary>
        /// 链上提现提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChainWithDrawSubmit")]
        public ActionResult<RespData<respPrepayData>> ChainWithDrawSubmit(chainWithDrawData data)
        {
            var asset = _assetService.GetAssetById(data.cid);
            if (data.amount < asset.withdraw_min)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("小于最小提现数量", lang));
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);

            var gift = _user.GetTotalUserGift(userbalance.uid);
            gift = gift * 0.1;

            if (data.amount>(userbalance.balance_reward-userbalance.frozen_reward-gift))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("余额不足", lang));
            var psn = _prepayService.InsertPrepayAsset(ticket.userId, prepayType.Withdrawal, data.amount, asset.withdraw_fee, receiveAddr: data.address);
            if (string.IsNullOrWhiteSpace(psn))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("创建不成功"));
            return geneRetData.geneRate<respPrepayData>(1, new respPrepayData
            {
                prepay_id = psn,
                p_type = (int)prepayType.Withdrawal,
                verify = 2
            });
        }
        /// <summary>
        /// 普通提现界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("WithDrawView")]
        public ActionResult<RespData<withdrawView>> WithDrawView()
        {
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);
            var asset = _assetService.GetAssetById(1);
            var user = _user.GetUserByUsnNotCache(userbalance.usn);
            var p_list = _payment.GetUPayMentListByUid(user.uid);
            var list = new List<upayMentTitle>();
            foreach (var item in p_list)
            {
                list.Add(new upayMentTitle
                {
                    icon = item.icon,
                    account = item.account,
                    active = user.d_payment == item.id ? 1 : 0,
                    payname = item.pay_name,
                    paysn = item.guid,
                    paytitle = lang == "cn" ? item.pay_cnname : item.pay_name
                });
            }

            return geneRetData.geneRate<withdrawView>(1, new withdrawView
            {
                title = "提现",
                //unit = asset.c_unit,
                unit="CNY",
                fee = asset.withdraw_fee,
                min = asset.withdraw_min,
                balance = userbalance.balance_shopping,
                detailTips = "",
                list=list
            });
        }

        /// <summary>
        /// 普通提现提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("WithDrawSubmit")]
        public ActionResult<RespData<respPrepayData>> WithDrawSubmit(reqWithdrawData data)
        {
            var asset = _assetService.GetAssetById(1);
            if (data.amount < asset.withdraw_min)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("小于最小提现数量", lang));
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);

            var psn = _prepayService.InsertPrepayAsset(ticket.userId, prepayType.Withdrawal, data.amount,asset.withdraw_fee,receiveAddr:data.paysn);
            if (string.IsNullOrWhiteSpace(psn))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("创建不成功"));
            return geneRetData.geneRate<respPrepayData>(1, new respPrepayData
            {
                prepay_id=psn,
                p_type=(int)prepayType.Withdrawal,
                verify=2
            });
        }
        /// <summary>
        /// 站内互转界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("TransferView")]
        public ActionResult<RespData<transferViewModel>> TransferView()
        {
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);
            var balance = userbalance.balance_reward - userbalance.frozen_reward;
            return geneRetData.geneRate<transferViewModel>(1, new transferViewModel
            {
                balance=balance
            });
        }
        /// <summary>
        /// 站内互转提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("TransferSubmit")]
        public ActionResult<RespData<respPrepayData>> TransferSubmit(reqTransfer data)
        {
            if (string.IsNullOrWhiteSpace(data.phone) || string.IsNullOrWhiteSpace(data.phonearea))
                throw new LionException("电话或区号不能为空", 404);
            var phonearea = StringHelper.GetNumberFromStr(data.phonearea);
            var receive = _user.GetUserByPhone(data.phone, phonearea);
            if (receive == null)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("找不到用户", lang));
            var userbalance = _user.GetBalanceByUserSN(ticket.userId);
            if(data.amount>(userbalance.balance_reward-userbalance.frozen_reward))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("余额不足此次交易", lang));

            var psn = _prepayService.InsertPrepayAsset(ticket.userId, prepayType.Transfer, data.amount,receiveSn:receive.guid);
            if (string.IsNullOrWhiteSpace(psn))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("创建不成功"));
            return geneRetData.geneRate<respPrepayData>(1, new respPrepayData
            {
                prepay_id = psn,
                p_type = (int)prepayType.Transfer,
                verify = 2
            });
        }
        /// <summary>
        /// 转充值界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefillView")]
        public ActionResult<RespData<rewardToShoppingView>> RefillView()
        {
            var balance = _user.GetBalanceByUserSN(ticket.userId);
            var rewardbalance = balance.balance_reward - balance.frozen_reward;
            var unit = _assetService.GetIntegralMain().c_unit;
            return geneRetData.geneRate<rewardToShoppingView>(1, new rewardToShoppingView
            {
                rewardBalance=_commonService.CutAmount(rewardbalance,4),
                rewardTips="奖励余额介绍",
                shoppingTips="购物余额介绍",
                unit=unit
            });
        }
        /// <summary>
        /// 转充值提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefillSubmit")]
        public ActionResult<RespData<respPrepayData>> RefillSubmit(reqRefillData data)
        {
            if(data.amount<=0)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("数量须大于零", lang));
            var balance = _user.GetBalanceByUserSN(ticket.userId);
            var rewardbalance = balance.balance_reward - balance.frozen_reward;
            if (data.amount>rewardbalance)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("余额不足此次交易", lang));

            var psn = _prepayService.InsertPrepayAsset(ticket.userId, prepayType.Reward,data.amount);
            if (string.IsNullOrWhiteSpace(psn))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("创建不成功"));
            return geneRetData.geneRate<respPrepayData>(1, new respPrepayData
            {
                prepay_id = psn,
                p_type = (int)prepayType.Reward,
                verify = 1
            });
        }

        /// <summary>
        /// 设置闪兑比例
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetFlash")]
        public ActionResult<RespData<bool>> SetFlash(flashData data)
        {
            if (data.rate < 0 || data.rate % 20 != 0 || data.rate>100)
                return geneRetData.geneRate<bool>(1401, false, "设置数值不正确");
            var user = _user.GetUserByUserSN(ticket.userId);
            int count = _user.SetFlash(user.uid, data.rate);
            if (count > 0)
                return geneRetData.geneRate<bool>(1, true);
            return geneRetData.geneRate<bool>(1, false);
        }
        /// <summary>
        /// 均价回购界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("AvgBuyView")]
        public ActionResult<RespData<avgBuyViewModel>> AvgBuyView()
        {
            var user = _user.GetUserByUserSN(ticket.userId);
            var wallet = _user.GetWalletByUid(user.uid);
            var asset = _assetService.GetIntegralMain();
            return geneRetData.geneRate<avgBuyViewModel>(1, new avgBuyViewModel
            {
                balance=_commonService.CutAmount(wallet.balance,4),
                price=0.1,
                unit=asset.c_unit
            });
        }
        /// <summary>
        /// 均价回购提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("AvgBuySubmit")]
        public ActionResult<RespData<respPrepayData>> AvgBuySubmit(avgBuyData data)
        {
            if (data.amount <= 0)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("数量不能为0"));
            var user = _user.GetUserByUserSN(ticket.userId);
            var wallet = _user.GetWalletByUid(user.uid);
            if (data.amount > wallet.balance)
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("余额不足", lang));
            var price = _assetService.GetIssuePrice();
            var psn = _prepayService.InsertPrepayAsset(ticket.userId, Lion.Entity.prepayType.PlatBuy,data.amount,
                rate:price);
            if (string.IsNullOrWhiteSpace(psn))
                return geneRetData.geneRate<respPrepayData>(1401, null,
                    _lang.GetLangByTitle("创建不成功"));
            return geneRetData.geneRate<respPrepayData>(1, new respPrepayData
            {
                prepay_id = psn,
                p_type = (int)prepayType.PlatBuy,
                verify = 1
            });
            //_user.PlatBuy(user.uid, data.amount);
            //return geneRetData.geneRate<bool>(1,true);
        }
        /// <summary>
        /// 数据中心
        /// </summary>
        /// <returns></returns>
        [HttpPost("DataCenter")]
        public ActionResult<RespData<DCViewModel>> DataCenter()
        {
            var model = _user.GetDataCenterView();
            return geneRetData.geneRate<DCViewModel>(1, model);
        }
    }
}