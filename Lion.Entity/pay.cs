using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    /// <summary>
    /// 资产预付表用于 站内互转,普通提现,场外出售,余额类型互转等等
    /// </summary>
    [Table("lt_prepayasset")]
    public class prepayassetEntity
    {
        [Key]
        public int pid { get; set; }
        public string psn { get; set; }
        public string usn { get; set; }
        public int p_type { get; set; }
        public string out_trade_no { get; set; }
        public int asset_code { get; set; }
        public int currency_id { get; set; }
        /// <summary>
        /// 接收者usn
        /// </summary>
        public string receive_sn { get; set; }
        public string receive_addr { get; set; }
        public double amount { get; set; }
        public double fee { get; set; }
        public double rate { get; set; }
        public int flag { get; set; }
        public long times { get; set; }
    }
    [Table("lt_goodscanc")]
    public class goodscancEntity
    {
        public string gsn { get; set; }
        public string usn { get; set; }
        public string pro_name { get; set; }
        public string psn { get; set; }
        public double camount { get; set; }
        public double rate { get; set; }
        public double amount { get; set; }
        public long times { get; set; }
    }
    [Table("lt_shoppingrecord")]
    public class shoprecordEntity
    {
        /// <summary>
        /// 预付单号
        /// </summary>
        public string psn { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        public string out_trade_no { get; set; }
        /// <summary>
        /// 付款编号
        /// </summary>
        public string pay_no { get; set; }
        public int asset_code { get; set; }
        public int currency_id { get; set; }
        public double amount { get; set; }
        public double camount { get; set; }
        public double rate { get; set; }
        public long times { get; set; }
    }
}
