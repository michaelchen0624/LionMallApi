using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    /// <summary>
    /// 普通提现记录表
    /// </summary>
    [Table("lt_withdraw")]
    public class withdrawEntity
    {
        [Key]
        public int id { get; set; }
        public string guid { get; set; }
        public string usn { get; set; }
        public string receive { get; set; }
        public int addr_id { get; set; }
        public string receive_addr { get; set; }
        public int asset_id { get; set; }
        public double amount { get; set; }
        public double fee { get; set; }
        public int flag { get; set; }
        public long h_time { get; set; }
        public string h_ret { get; set; }
        public double h_fee { get; set; }
        public int del { get; set; }
        public long times { get; set; }
    }
}
