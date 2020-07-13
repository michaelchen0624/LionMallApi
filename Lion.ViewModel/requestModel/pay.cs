using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lion.ViewModel.requestModel
{
    public class reqDate
    {
        public string from { get; set; }
        public string to { get; set; }
        public string sign { get; set; }
    }
    public class reqGoodsOrder
    {
        [Required]
        public string psn { get; set; }
        [Required]
        public string usn { get; set; }
        public int camount { get; set; }
        [Required]
        public string productName { get; set; }
        [Required]
        public string sign { get; set; }
    }

    public class reqUnifiedOrder
    {
        /// <summary>
        /// 商户号
        /// </summary>
        [Required]
        public string mch_id { get; set; }
        [Required]
        public string usn { get; set; }
        public string product { get; set; }
        /// <summary>
        /// 随机字符串
        /// </summary>
        [Required]
        public string nonce_str { get; set; }
        [Required]
        public string sign { get; set; }
        //[Required]
        //public string out_trade_no { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        public string order_no { get; set; }
        /// <summary>
        /// 付款币种
        /// </summary>
        [Required]
        public string currency { get; set; }
        //[Required]
        //public string notify_url { get; set; }
        /// <summary>
        /// 付款金额
        /// </summary>
        public int total_amount { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public long order_time { get; set; }

    }
    public class reqPayVerify
    {
        /// <summary>
        /// 预付单号
        /// </summary>
        [Required]
        public string prepay_id { get; set; }
        public string pwd { get; set; }
        public string msgsign { get; set; }
        public string msgcode { get; set; }
        public int p_type { get; set; }
        public int verify { get; set; }
    }
    public class reqWithdrawData
    {
        //[Required]
        //public string receive_addr { get; set; }
        public string paysn { get; set; }
        public double amount { get; set; }
        public double fee { get; set; }
    }
    public class chainWithDrawData
    {
        public int cid { get; set; }
        public string address { get; set; }
        public double amount { get; set; }
    }
}
