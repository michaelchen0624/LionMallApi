using Lion.ViewModel.requestModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel.respModel
{
    public class marketTradeFlag
    {
        /// <summary>
        /// 有未处理单据
        /// </summary>
        public bool pendingFlag { get; set; }
        public bool paymentFlag { get; set; }
        public tmpOrder order { get; set; }
    }
    public class userBalance
    {
        public double balance { get; set; }
        public double currency_balance { get; set; }
    }
    /// <summary>
    /// 钱包界面
    /// </summary>
    public class walletView
    {
        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 总余额
        /// </summary>
        public string totalBalance { get; set; }
        public string buyBalance { get; set; }
        public string rewardBalance { get; set; }
        //public int userLevel { get; set; }
        //public string levelName { get; set; }
        //public string imgUrl { get; set; }
    }
    public class depositView
    {
        public string title { get; set; }
        public string address { get; set; }
        public string tips { get; set; }
        public string[] chainType { get; set; }
    }
    public class depoistView
    {
        public IList<depositTypeItem> purchase { get; set; }
        public IList<depositTypeItem> withdrawal { get; set; }
    }
    public class depositTypeItem
    {
        public int id { get; set; }
        public string c_name { get; set; }
        public int coinType { get; set; }
    }
    public class CoinTypeData
    {
        public int id { get; set; }
    }
    /// <summary>
    /// c2c购买界面
    /// </summary>
    public class marketBuyView
    {
        public string title { get; set; }
        public string pricestr { get; set; }
        public double price { get; set; }
        /// <summary>
        /// 数量限额提示 限额1000-2000单位
        /// </summary>
        public string countTips { get; set; }
        //public string quotaTips { get; set; }
        /// <summary>
        /// 金额限额提示 限额 5000-20000
        /// </summary>
        //public string amountTips { get; set; }
        /// <summary>
        /// 最大单笔购买数量
        /// </summary>
        public double maxCount { get; set; }
        /// <summary>
        /// 最小单笔购买数量
        /// </summary>
        public double minCount { get; set; }
        //public double maxAmount { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 取消秒数
        /// </summary>
        public int cancelSecond { get; set; }
        public string currencyCode { get; set; }
        public string symbol { get; set; }
        public double feeRatio { get; set; }

    }
    /// <summary>
    /// c2c购买确认界面
    /// </summary>
    public class marketBuyViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int orderFlag { get; set; }
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
        /// 申诉状态
        /// </summary>
        public int appealStatus { get; set; }
        /// <summary>
        /// 付款方式列表
        /// </summary>
        public IList<payment> paymentList { get; set; }
    }
    public class upaymentViewModel
    {
        public string usn { get; set; }
        public IList<payment> p_list { get; set; }
    }

    /// <summary>
    /// 付款方式
    /// </summary>
    public class payment
    {
        public int payType { get; set; }
        /// <summary>
        /// 支付方式名称
        /// </summary>
        public string payName { get; set; }
        /// <summary>
        /// 支付方式标题
        /// </summary>
        public string payTitle { get; set; }
        /// <summary>
        /// 收款人
        /// </summary>

        public string payee { get; set; }
        /// <summary>
        /// 收款账号
        /// </summary>
        public string accountNum { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        public string bank { get; set; }
        /// <summary>
        /// 二维码
        /// </summary>
        public string qrCode { get; set; }
    }
    public class chainWithDrawView
    {
        public string title { get; set; }
        public string unit { get; set; }
        public string[] chainType { get; set; }
        /// <summary>
        /// 最小提现数量提示
        /// </summary>
        //public string minTips { get; set; }
        public string balance { get; set; }

        public double min { get; set; }
        public double fee { get; set; }
        public string detailTips { get; set; }
    }
    /// <summary>
    /// 提现界面
    /// </summary>
    public class withdrawView
    {
        public string title { get; set; }
        public string unit { get; set; }
        /// <summary>
        /// 最小提现数量提示
        /// </summary>
        //public string minTips { get; set; }
        public double balance { get; set; }

        public double min { get; set; }
        public double fee { get; set; }
        public string detailTips { get; set; }
        public IList<upayMentTitle> list { get; set; }
    }
    /// <summary>
    /// C2C出售界面
    /// </summary>
    public class marketSellView
    {
        public string title { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public double price { get; set; }
        public string balance { get; set; }
        ///// <summary>
        ///// 数量限额提示 限额1000-2000单位
        ///// </summary>
        public string countTips { get; set; }
        /// <summary>
        /// 最大单笔购买数量
        /// </summary>
        public double maxCount { get; set; }
        /// <summary>
        /// 最小单笔购买数量
        /// </summary>
        public double minCount { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// CNT
        /// </summary>
        public string currencyCode { get; set; }
        /// <summary>
        /// 符号
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double feeRatio { get; set; }

    }

    /// <summary>
    /// c2c出售确认界面
    /// </summary>
    public class marketSellViewModel
    {
        /// <summary>
        /// 单据类型
        /// </summary>
        public int orderFlag { get; set; }
        public string title { get; set; }
        public string buyerPhone { get; set; }
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
        /// 申诉状态
        /// </summary>
        public int appealStatus { get; set; }
        ///// <summary>
        ///// 付款方式列表
        ///// </summary>
        //public IList<payment> paymentList { get; set; }
        /// <summary>
        /// 付款人姓名
        /// </summary>
        public string payerName { get; set; }
    }
    public class rewardToShoppingView
    {
        public string rewardTips { get; set; }
        public string shoppingTips { get; set; }
        public string rewardBalance { get; set; }
        public string unit { get; set; }
    }
    public class transferViewModel
    {
        public double balance { get; set; }
    }
    public class avgBuyViewModel
    {
        public double price { get; set; }
        public string balance { get; set; }
        public string unit { get; set; }
    }
    public class DCViewModel
    {
        /// <summary>
        /// 今个新增用户数
        /// </summary>
        public int current_reg { get; set; }
        /// <summary>
        /// 总用户数
        /// </summary>
        public int total_reg { get; set; }
        /// <summary>
        /// 单位 usdt
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 昨日销售额
        /// </summary>
        public string last_consum { get; set; }
        /// <summary>
        /// 总销售额
        /// </summary>
        public string total_consum { get; set; }
        /// <summary>
        /// 昨日利润
        /// </summary>
        public string last_profit { get; set; }
        /// <summary>
        /// 总利润
        /// </summary>
        public string total_profit { get; set; }
        /// <summary>
        /// 昨个发行
        /// </summary>
        public string last_issue { get; set; }
        /// <summary>
        /// 累计发行总量
        /// </summary>
        public string total_issue { get; set; }
        /// <summary>
        /// 昨个发行价格
        /// </summary>
        public double[] issue_price { get; set; }
        public string[] issue_date { get; set; }
        /// <summary>
        /// 昨个回购价格
        /// </summary>
        public double [] buy_price { get; set; }
        /// <summary>
        /// 昨个回购
        /// </summary>
        public string[] buy_date { get; set; }
        /// <summary>
        /// 昨个闪兑
        /// </summary>
        public string last_flash { get; set; }
        /// <summary>
        /// 累计闪兑
        /// </summary>
        public string total_flash { get; set; }
        /// <summary>
        /// 昨个均价回购
        /// </summary>
        public string last_avg { get; set; }
        /// <summary>
        /// 累计均价回购
        /// </summary>
        public string total_avg { get; set; }
        /// <summary>
        /// 累计回购
        /// </summary>
        public string total_buy { get; set; }
        /// <summary>
        /// 回购基金余额
        /// </summary>
        public string balance { get; set; }
    }
}
