using Lion.ViewModel.requestModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel.respModel
{
    public class respPrepayData
    {
        public string prepay_id { get; set; }
        public int p_type { get; set; }
        public int verify { get; set; }
    }
    public class respNotiry
    {
        public string mch_id { get; set; }
        public string sign { get; set; }
        public string out_trade_no { get; set; }
        public double rate { get; set; }
        public string order_no { get; set; }
        public double camount { get; set; }
    }
    public class respGoodsCanc
    {
        public string order_sn { get; set; }
    }
    public class respPrepay
    {
        public string mch_id { get; set; }
        public string user_sn { get; set; }
        public string nonce_str { get; set; }
        public string sign { get; set; }
        public string order_no { get; set; }
        /// <summary>
        /// 预付单号
        /// </summary>
        public string prepay_id { get; set; }
        public int p_type { get; set; }
        public int verify { get; set; }
    }
    public class pwdVerifyViewModel
    {
        public string title { get; set; }
        public string prepay_id { get; set; }
        public int p_type { get; set; }
        public string symbol { get; set; }
        public string amount { get; set; }
        public IList<titleContent> subItems { get; set; }

    }
    public class msgVerifyViewModel
    {
        public int expireSecond { get; set; }
        public string msgsign { get; set; }
        public int step { get; set; }
    }
    public class msgVerifyRet
    {
        public int expire { get; set; }
        public int step { get; set; }
        public bool ret { get; set; }
        public tmpOrder order { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class titleContent
    {
        public string title { get; set; }
        public string content { get; set; }
    }
    /// <summary>
    /// 兑换商预付单号返回值
    /// </summary>
    public class respChangerPrepay
    {
        public string prepay_id { get; set; }
    }
    public class respNewUser
    {
        public string from { get; set; }
        public string to { get; set; }
        public IList<respNewUserItem> list { get; set; }
        public int totalCount { get; set; }
    }
    public class respNewUserItem
    {
        public string phonearea { get; set; }
        public string phone { get; set; }
        public string username { get; set; }
        public string level { get; set; }
        public string totalConsum { get; set; }
        public string regTime { get; set; }
    }
    public class userGift
    {
        public string from { get; set; }
        public string to { get; set; }
        public IList<userGiftItem> list { get; set; }
        public int totalCount { get; set; }
    }
    public class userGiftItem
    {
        public string phonearea { get; set; }
        public string phone { get; set; }
        public string amount { get; set; }
        public string times { get; set; }
    }
    public class respNewIssue
    {
        public string from { get; set; }
        public string to { get; set; }
        public IList<NewIssue> list { get; set; }
    }
    public class NewIssue
    {
        public string date { get; set; }
        /// <summary>
        /// 充值
        /// </summary>
        public string recharge { get; set; }
        /// <summary>
        /// 发行量
        /// </summary>
        public string issue { get; set; }
        /// <summary>
        /// 闪兑回购量
        /// </summary>
        public string flash { get; set; }
        /// <summary>
        /// 闪兑回购折合
        /// </summary>
        public string flash_us { get; set; }

    }
}
