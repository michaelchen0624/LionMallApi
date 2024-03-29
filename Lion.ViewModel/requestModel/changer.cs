﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Text;

namespace Lion.ViewModel.requestModel
{
    public class reqChainData
    {
        public string usn { get; set; }
        public int plat { get; set; }
        public string sign { get; set; }
    }

    public class reqAddress 
    {
    public string pwd { get; set; }
    }
    /// <summary>
    /// 场外购买提交
    /// </summary>
    public class marketBuyData
    {
        [Range(typeof(double),"10.00","2000.00",ErrorMessage ="购买数量应在10.00-2000.00之间")]
        public double count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage ="货币类型不能为空")]
        public string currencyCode { get; set; }
    }
    public class tmpOrderNo
    {
        public orderType tmpType { get; set; }
        public string orderId { get; set; }
    }
    public class tmpOrder
    {
        public orderType tmpType { set; get; }
        public string orderId { get; set; }
        public int orderFlag { get; set; }
    }
    public class respTmpOrder
    {
        public orderType tmpType { get; set; }
        public string orderId { get; set; }
        public int orderFlag { get; set; }
        public long expireTime { get; set; }
    }
    /// <summary>
    /// 支付方式guid
    /// </summary>
    public class paySN
    {
        public string paysn { get; set; }
    }
    public class payActive
    {
        public string paysn { get; set; }
        public int active { get; set; }
    }
    /// <summary>
    /// 添加收款方式
    /// </summary>
    public class addPaymentData
    {
        public string paysn { get; set; }
        public string bank { get; set; }
        [Required(ErrorMessage ="收款人不能为空")]
        public string payee { get; set; }
        [Required(ErrorMessage ="账号不能为空")]
        public string account { get; set; }
        public string qrcode { get; set; }
    }
    public class orderInquiry
    {
        public inquiryType inquiry { get; set; }
    }
    public enum inquiryType
    {
        paymentMade=1//已付款
    }

    public class marketSellData
    {
        [Range(typeof(double), "10.00", "2000.00", ErrorMessage = "购买数量应在10.00-2000.00之间")]
        public double count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "货币类型不能为空")]
        public string currencyCode { get; set; }
    }
    public enum orderType
    {
        marketBuy=1,
        marketSell=2
    }
    public enum orderFlagType
    {
        请付款=21,
        已付款=22,
        订单申诉=23,
        标记已完成=24,
        标记已取消=25,
        请收款=41,
        出售订单申诉=42,
        出售标记完成=43,
        出售标记取消=44
    }
    public class reqMsgData
    {
        public string Mobile { get; set; }
        public string Msg { get; set; }
        public string sign { get; set; }
        public string TemplateCode { get; set; }
        public string AreaCode { get; set; }
    }
    /// <summary>
    /// 提交到兑换商的统一下单接口
    /// </summary>
    public class reqChangerUnifiedorder
    {
        public string out_trade_no { get; set; }
        public double amount { get; set; }
        //public int rate { get; set; }
        public string usn { get; set; }
        public string uphone { get; set; }
        /// <summary>
        /// 1=用户购买，2=用户出售
        /// </summary>
        public int order_type { get; set; }
        public string currency_code { get; set; }
        public string sign { get; set; }
    }
    /// <summary>
    /// 兑换商回调数据
    /// </summary>
    public class notifyOrderStatusData
    {
        public string ordersn { get; set; }
        /// <summary>
        /// 外部单号
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// success,cancel
        /// </summary>
        public string status { get; set; }
        public string usn { get; set; }
        public double fee { get; set; }
        public double camount { get; set; }
        public double rate { get; set; }
        public string sign { get; set; }
        public int orderType { get; set; }
        /// <summary>
        /// 取消 1=兑换商 2=用户
        /// </summary>
        public int initiator { get; set; }
        public long h_time { get; set; }
    }

    /// <summary>
    /// 兑换商回调数据
    /// </summary>
    public class notifyData
    {
        /// <summary>
        /// 外部订单号
        /// </summary>
        public string txhash { get; set; }
    }

    /// <summary>
    /// 链上提现请求数据
    /// </summary>
    public class reqChainWithdrawData
    {
        public string pwd { get; set; }
        public string keystore { get; set; }
        public string toAddress { get; set; }
        public string money { get; set; }
        public string gwei { get; set; }
        public string token { get; set; }
    }

    public class NotifyPayData
    {
        public string usn { get; set; }
        public string address { get; set; }
        public string asset { get; set; }
        public string from { get; set; }
        public string txid { get; set; }
        public string amount { get; set; }
        public long times { get; set; }
        public string sign { get; set; }
    }
    public class RqTradeHis
    {
        public string userName { get; set; }
        public string begindate { get; set; }
        public string enddate { get; set; }
        public int Type { get; set; }
        public int page { get; set; }
        public int pagelimit { get; set; }
        public string sign { get; set; }
    }
    public class TradeRecord
    {
        public int id { get; set; }
        /// <summary>
        /// 单据guid
        /// </summary>
        public string orderid { get; set; }
        /// <summary>
        /// 单号 10位数字
        /// </summary>
        public string orderno { get; set; }
        /// <summary>
        /// 兑换商服务单据id
        /// </summary>
        public string prepaysn { get; set; }
        public int uid { get; set; }
        /// <summary>
        /// 兑换商id
        /// </summary>
        public int cid { get; set; }
        /// <summary>
        /// 支付方式id
        /// </summary>
        public int paymentid { get; set; }
        /// <summary>
        /// 购买金额
        /// </summary>
        public double amount { get; set; }
        /// <summary>
        /// 付款币种id
        /// </summary>
        public int currencyid { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double rate { get; set; }
        /// <summary>
        /// 对应付款币种的付款金额
        /// </summary>
        public double camount { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double fee { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 兑换商确认时间
        /// </summary>
        public long confirmtime { get; set; }
        /// <summary>
        /// 完成标志1=已完成
        /// </summary>
        public int flag { get; set; }
        /// <summary>
        /// 单据创建时间
        /// </summary>
        public long times { get; set; }
    }
    public class TradeHis
    {
        public string orderid { get; set; }
        public string orderno { get; set; }
        public string prepaysn { get; set; }
        public string username { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public double rate { get; set; }
        public double camount { get; set; }
        public double fee { get; set; }
        public long confirmtime { get; set; }
        public string type { get; set; }
        public long time { get; set; }
        public string state { get; set; }
    }
    public class RspTradeHis
    {
        public int count { get; set; }
        public List<TradeHis> list { get; set; }
    }
}
