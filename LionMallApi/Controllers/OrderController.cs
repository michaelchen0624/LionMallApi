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
using Newtonsoft.Json;
using LionMall.Tools;
using Lion.Entity;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private OrderService _orderService;
        private ILogService _log;
        private AssetService _assetService;
        private IConfiguration _config;
        private RpcNotifyService _rpcService;
        private TicketService _ticketService;
        private UserService _userService;
        private ChangerService _changerService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderService"></param>
        /// <param name="log"></param>
        /// <param name="assetService"></param>
        /// <param name="config"></param>
        /// <param name="rpcService"></param>
        /// <param name="ticketService"></param>
        /// <param name="userService"></param>
        /// /// <param name="changerService"></param>
        public OrderController(OrderService orderService,ILogService log,AssetService assetService,
            IConfiguration config,TicketService ticketService,UserService userService,RpcNotifyService rpcService, ChangerService changerService)
        {
            _orderService = orderService;
            _log = log;
            _assetService = assetService;
            _config = config;
            _rpcService = rpcService;
            _ticketService = ticketService;
            _userService = userService;
            _changerService = changerService;
        }

        /// <summary>
        /// 短信发送测试
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("ChainNotityTest")]
        public ActionResult<string> ChainNotityTest(NotifyPayData data)
        {
            var paydata = new PayData<NotifyPayData>(data);
            if (!paydata.CheckSign(PaySignType.MD5, "1234"))
                return "signerror";
            _log.I("chainNotifyTest", JsonConvert.SerializeObject(data));
            Task.Run(() =>
            {
                _orderService.HandleChainTest(data);
            });
            return "success";
        }

        /// <summary>
        /// 计算层级
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("LevelCal")]
        public ActionResult<RespData<int>> LevelCal(reqUsn data)
        {
            var userextend = _userService.GetUserExtendByUsn(data.usn);
            if(userextend==null)
            {
                return geneRetData.geneRate<int>(1401, 0, "用户不存在");
            }
            var cal= _userService.LevelCalculate(userextend.uid, false);
            return geneRetData.geneRate<int>(1, cal);
        }
        /// <summary>
        /// 支付请求
        /// </summary>
        /// <returns></returns>
        [HttpPost("PayMent")]
        public ActionResult<RespData<string>> PayMent(payItem item)
        {
            return geneRetData.geneRate<string>(1, "");
        }
        /// <summary>
        /// 支付确认
        /// </summary>
        /// <returns></returns>
        [HttpPost("Confirm")]
        public ActionResult<RespData<string>> PayConfirm()
        {
            return geneRetData.geneRate<string>(1, "");
        }
        /// <summary>
        /// 虚拟货币支付
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("ChainNotityPay")]
        public ActionResult<string> ChainNotityPay(NotifyPayData data)
        {
            var paydata = new PayData<NotifyPayData>(data);
            if (!paydata.CheckSign(PaySignType.MD5, "1234"))
                return "signerror";
            _log.I("chainNotify", JsonConvert.SerializeObject(data));
            Task.Run(() =>
            {
                _orderService.HandleChainOrder(data);
            });
            return "success";
        }
        /// <summary>
        /// 兑换商回调
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangerOrder")]
        public ActionResult<string> ChangerOrder(notifyOrderStatusData data)
        {
            var paydata = new PayData<notifyOrderStatusData>(data);
            if (!paydata.CheckSign(PaySignType.MD5, "1234"))
                return "signerror";
            _log.I("orderNotify", JsonConvert.SerializeObject(data));
            Task.Run(() =>
            {
                _orderService.HandleChangerOrder(data.out_trade_no, data.usn, data.orderType,
                    data.status, data.h_time, data.camount, data.fee, data.rate, data.initiator);
            });
            return "success";
        }

        /// <summary>
        /// 场外充值回调
        /// </summary>
        /// <returns></returns>
        [HttpPost("ExchangeBuyNotify")]
        public ActionResult<string> ExchangeBuyNotify(notifyData data)
        {
            _log.I("ExchangeBuyNotify", JsonConvert.SerializeObject(data));
            Task.Run(() =>
            {
                _orderService.ExchangeBuyOrder(data.txhash);
            });
            return "success";
        }

        /// <summary>
        /// 链上提现回调
        /// </summary>
        /// <returns></returns>
        [HttpPost("ExchangeSellNotify")]
        public ActionResult<string> ExchangeSellNotify(notifyData data)
        {
            _log.I("ExchangeSellNotify", JsonConvert.SerializeObject(data));
            Task.Run(() =>
            {
                _orderService.ExchangeSellOrder(data.txhash);
            });
            return "success";
        }

        /// <summary>
        /// 获取交易记录
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOutTradeHis")]
        public ActionResult<RespData<object>> GetOutTradeHis(string address)
        {
            var addr = _rpcService.GetOutTradeHis(address);
            return geneRetData.geneRate<object>(1, addr);
        }

        /// <summary>
        /// 获取上传票据
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUpTicket")]
        public ActionResult<RespData<imgAuth>> GetUpTicket(upImgAuth auth)
        {
            //_log.I("test", JsonConvert.SerializeObject(auth));
            var imgType = (UpImgType)auth.imgType;
            var upconfigList = _config.GetSection("UpConfig").Get<List<upConfigItem>>();
            var upconfig = upconfigList.Where(o => o.contentType.Equals(imgType.ToString(),
                  StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (upconfig == null)
                return geneRetData.geneRate<imgAuth>(1401, null, "Up Type error");
            var paydata = new PayData<upImgAuth>(auth);
            paydata.CheckSign(PaySignType.MD5, upconfig.secureKey);
            var ret= _ticketService.GetUpTicket(imgType);
            return geneRetData.geneRate<imgAuth>(1, new imgAuth
            {
                Authorization = ret
            });
        }

        /// <summary>
        /// 获取交易记录
        /// </summary>
        /// <returns></returns>
        [HttpPost("TradeList")]
        public ActionResult<RespData<RspTradeHis>> TradeList(RqTradeHis data)
        {
            _log.I("TradeList", JsonConvert.SerializeObject(data));
            var paydata = new PayData<RqTradeHis>(data);
            if (!paydata.CheckSign(PaySignType.MD5, "1234"))  
                return geneRetData.geneRate<RspTradeHis>(1401, 0, new RspTradeHis(), "签名错误");
            var list = _changerService.GetTradeList(data);
            return geneRetData.geneRate<RspTradeHis>(1, list);
        }

        /// <summary>
        /// 获取推广二维码
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetQrCode")]
        public ActionResult<RespData<respInviteUrl>> GetQrCode(reqUsn data)
        {
            //_log.I("usertest", data.usn);
            var user = _userService.GetUserByUserSN(data.usn);
            var url = _config.GetValue<string>("OrderConfig:regUrl");
            var qrCode = $"{url}?recommend={user.recommend}";
            //_log.I("usertest", qrCode);
            return geneRetData.geneRate<respInviteUrl>(1, new respInviteUrl
            {
                qrCode = qrCode
            });
        }
        /// <summary>
        /// 更改货币地址
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        //[HttpPost("ChangeAddress")]
        //public ActionResult<RespData<string>> ChangeAddress(reqUsn data)
        //{
        //    var user = _userService.GetUserByUserSN(data.usn);
        //    if (user == null)
        //        return geneRetData.geneRate<string>(1401, null, "找不到用户");
        //    var addr = _rpcService.GetNewAddr(new reqChainData
        //    {
        //        plat=2,
        //        usn=user.guid
        //    });
        //    if(string.IsNullOrWhiteSpace(addr))
        //        return geneRetData.geneRate<string>(1401, null, "生成地址失败");
        //    _assetService.UpdateRechargeAddr(user.uid, addr);
        //    return geneRetData.geneRate<string>(1, "");
        //}

        /// <summary>
        /// 更改货币地址
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("ChangeAddress")]
        public ActionResult<RespData<string>> ChangeAddress(reqUsn data)
        {
            var user = _userService.GetUserByUserSN(data.usn);
            if (user == null)
                return geneRetData.geneRate<string>(1401, null, "找不到用户");
            var addr = _rpcService.GetAddr(new reqAddress
            {
                pwd = data.usn
            });
            if (string.IsNullOrWhiteSpace(addr))
                return geneRetData.geneRate<string>(1401, null, "生成地址失败");
            _assetService.UpdateRechargeAddr(user.uid, addr);
            return geneRetData.geneRate<string>(1, "");
        }
    }
}