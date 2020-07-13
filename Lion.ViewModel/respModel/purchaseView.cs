using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel.respModel
{
    /// <summary>
    /// 已付款界面
    /// </summary>
    public class paymentMadeViewModel
    {
        public string title { get; set; }
        public string sellerPhone { get; set; }
        /// <summary>
        /// 付款金额
        /// </summary>
        public string amount { get; set; }
        public string orderno { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string price { get; set; }
        /// <summary>
        /// 货币符号
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string count { get; set; }
        /// <summary>
        /// 单位 usdt,btc
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 提示内容
        /// </summary>
        public string detailTips { get; set; }
        /// <summary>
        /// 到期的秒数
        /// </summary>
        public long expiressTime { get; set; }
        /// <summary>
        /// 付款方式列表
        /// </summary>
        public IList<payment> paymentList { get; set; }
    }
    /// <summary>
    /// 订单申诉界面
    /// </summary>
    public class orderAppealViewModel
    {
        public string title { get; set; }
        public string sellerPhone { get; set; }
        /// <summary>
        /// 付款金额
        /// </summary>
        public string amount { get; set; }
        public string orderno { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string price { get; set; }
        /// <summary>
        /// 货币符号
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string count { get; set; }
        /// <summary>
        /// 单位 usdt,btc
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 提示内容
        /// </summary>
        public string detailTips { get; set; }
        ///// <summary>
        ///// 到期的秒数
        ///// </summary>
        //public long expiressTime { get; set; }
        /// <summary>
        /// 申诉状态 1=订单已完成 2=订单已取消
        /// </summary>
        public int appealStatus { get; set; }
        /// <summary>
        /// 付款方式列表
        /// </summary>
        public IList<payment> paymentList { get; set; }
    }
    public class inquiryStatus
    {
        public orderStatus orderstatus { get; set; }
    }
    public enum orderStatus
    {
        pending=20,
        success=1
    }
}
