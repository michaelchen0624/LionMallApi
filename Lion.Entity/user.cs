using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    [Table("lt_user")]
    public class userEntity
    {
        [Key]
        public int uid { get; set; }
        public string guid { get; set; }
        public string phone { get; set; }
        public string phonearea { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
        public string idnum { get; set; }
        public string passwd { get; set; }
        public string paypwd { get; set; }
        /// <summary>
        /// 推荐码十六进制的大写
        /// </summary>
        public string recommend { get; set; }
        /// <summary>
        /// 推荐人id
        /// </summary>
        public int origin { get; set; }
        public string iconurl { get; set; }
        /// <summary>
        /// 默认收款方式id
        /// </summary>
        public int d_payment { get; set; }
        public long times { get; set; }
    }
    [Table("lt_usercancel")]
    public class usercancelEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        public string ordersn { get; set; }
        public long times { get; set; }
    }
    [Table("lt_userextend")]
    public class userextendEntity
    {
        public int uid { get; set; }
        public string usn { get; set; }
        public int parent { get; set; }
        /// <summary>
        /// 青铜位消费
        /// </summary>
        public double bronze { get; set; }
        /// <summary>
        /// 级别
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// 累计消费额
        /// </summary>
        public double total_consum { get; set; }
    }
    public class subusercount
    {
        public int direct_count { get; set; }
        public int indirect_count { get; set; }
    }
    public class inviteuser
    {
        public int uid { get; set; }
        public string usn { get; set; }
        public int direct_count { get; set; }
        public string parent { get; set; }
        public string invite_code { get; set; }
    }
    public class userextend
    {
        public int uid { get; set; }
        public string usn { get; set; }
        public string username { get; set; }
        public int parent { get; set; }
        /// <summary>
        /// 级别
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// 累计消费额
        /// </summary>
        public double total_consum { get; set; }
        public string iconurl { get; set; }
        public string phonearea { get; set; }
        public string phone { get; set; }
    }
    public class user
    {
        public int uid { get; set; }
        public string guid { get; set; }
        public string phone { get; set; }
        public string phonearea { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
        public string idnum { get; set; }
        /// <summary>
        /// 推荐码十六进制的大写
        /// </summary>
        public string recommend { get; set; }
        public long times { get; set; }
    }
    /// <summary>
    /// 会员升级记录表
    /// </summary>
    [Table("lt_upgraderecord")]
    public class upgradeEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        /// <summary>
        /// 原级别
        /// </summary>
        public int origin_level { get; set; }
        /// <summary>
        /// 级别
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// 升级时间
        /// </summary>
        public long times { get; set; }
    }
    public class newuser
    {
        public string phonearea { get; set; }
        public string phone { get; set; }
        public string username { get; set; }
        public int level { get; set; }
        public double total_consum { get; set; }
        public long times { get; set; }
    }
    [Table("lt_gift")]
    public class giftEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        public string phonearea { get; set; }
        public string phone { get; set; }
        public double amount { get; set; }
        public long times { get; set; }
    }
    public class giftAmount
    {
        public double amount { get; set; }
    }
}
