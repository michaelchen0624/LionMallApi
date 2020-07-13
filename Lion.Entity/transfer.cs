using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    /// <summary>
    /// 站内互转
    /// </summary>
    [Table("lt_transfer")]
    public class transferEntity
    {
        [Key]
        public int id { get; set; }
        public int sender_id { get; set; }
        public string sender { get; set; }
        public int receive_id { get; set; }
        public string receive { get; set; }
        public double amount { get; set; }
        public long times { get; set; }
    }
    /// <summary>
    /// 奖励余额转购物余额列表
    /// </summary>
    [Table("lt_rechargeinternal")]
    public class rechargeinternalEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        public double amount { get; set; }
        public long times { get; set; }
    }
}
