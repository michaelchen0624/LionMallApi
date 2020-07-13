using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel.respModel
{
    /// <summary>
    /// 下单返回值
    /// </summary>
    public class respRetChangerPrepay
    {
        public string user_sn { get; set; }
        public string sign { get; set; }
        /// <summary>
        /// 用户展示单号
        /// </summary>
        public string order_no { get; set; }
        public string out_trade_no { get; set; }
        /// <summary>
        /// 预付单号
        /// </summary>
        public string prepay_id { get; set; }
        public int order_type { get; set; }
        //public int verify { get; set; }
    }
}
