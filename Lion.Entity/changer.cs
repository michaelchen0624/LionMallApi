using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    /// <summary>
    /// 兑换商
    /// </summary>
    [Table("lt_changer")]
    public class changerEntity
    {
        /// <summary>
        /// 兑换商id
        /// </summary>
        [Key]
        public int cid { get; set; }
        public string username { get; set; }
        public string fullname { get; set; }
        public string phone { get; set; }
        public string phonearea { get; set; }
        public string passwd { get; set; }
        public long times { get; set; }
    }
    /// <summary>
    /// 收款方式列表
    /// </summary>
    [Table("lt_payment")]
    public class paymentEntity
    {
        [Key]
        public int id { get; set; }
        public string guid { get; set; }
        /// <summary>
        /// 支付方式类型 1=银联卡类型，2=支付宝，微信
        /// </summary>
        public int pay_type { get; set; }
        public string pay_name { get; set; }
        public string pay_cnname { get; set; }
        public string icon { get; set; }
        public string area { get; set; }
        public int del { get; set; }
    }
    /// <summary>
    /// 用户收款方式
    /// </summary>
    [Table("lt_upayment")]
    public class upaymentEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 兑换商id
        /// </summary>
        public int uid { get; set; }
        public string guid { get; set; }
        /// <summary>
        /// payment的id
        /// </summary>
        public int payid { get; set; }
        /// <summary>
        /// 收款方式类型 1=银联卡类型，2=支付宝，微信
        /// </summary>
        public int pay_type { get; set; }
        /// <summary>
        /// 支付方式名称
        /// </summary>
        public string pay_name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pay_cnname { get; set; }
        /// <summary>
        /// 支付方式icon
        /// </summary>
        public string icon { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        public string bank { get; set; }
        /// <summary>
        /// 收款人
        /// </summary>
        public string payee { get; set; }
        /// <summary>
        /// 收款账号
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 收款二维码
        /// </summary>
        public string qrcode { get; set; }
        /// <summary>
        /// 主要收款方式标志
        /// </summary>
        public int mainflag { get; set; }
        /// <summary>
        /// 激活标志
        /// </summary>
        public int active { get; set; }
        /// <summary>
        /// 删除标志
        /// </summary>
        public int del { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long times { get; set; }
    }
    /// <summary>
    /// 兑换商收款方式
    /// </summary>
    [Table("lt_cpayment")]
    public class cpaymentEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 兑换商id
        /// </summary>
        public int cid { get; set; }
        /// <summary>
        /// payment的id
        /// </summary>
        public int payid { get; set; }
        /// <summary>
        /// 支付方式类型 1=银联卡类型，2=支付宝，微信
        /// </summary>
        public int pay_type { get; set; }
        /// <summary>
        /// 支付方式名称
        /// </summary>
        public string pay_name { get; set; }
        /// <summary>
        /// 支付方式中文名称
        /// </summary>
        public string pay_cnname { get; set; }
        /// <summary>
        /// 支付方式icon
        /// </summary>
        public string icon { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        public string bank { get; set; }
        /// <summary>
        /// 收款人
        /// </summary>
        public string payee { get; set; }
        /// <summary>
        /// 收款账号
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 收款二维码
        /// </summary>
        public string qrcode { get; set; }
        /// <summary>
        /// 主要收款方式标志
        /// </summary>
        public int mainflag { get; set; }
        /// <summary>
        /// 删除标志
        /// </summary>
        public int del { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long times { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    [Table("lt_currency")]
    public class currencyEntity
    {
        public int id { get; set; }
        public string c_name { get; set; }
        /// <summary>
        /// 简写
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 符号
        /// </summary>
        public string symbol { get; set; }
        public string icon { get; set; }
        public int del { get; set; }
    }
    [Table("lt_marketsellrecord")]
    public class marketsellrecordEntity
    {
        [Key]
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
        /// 用户付款时间
        /// </summary>
        public long cpaytime { get; set; }
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
    /// <summary>
    /// 场外购买记录表
    /// </summary>
    [Table("lt_marketbuyrecord")]
    public class marketbuyrecordEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 单据guid
        /// </summary>
        public string orderid { get; set; }
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
        /// 用户付款时间
        /// </summary>
        public long upaytime { get; set; }
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
    /// <summary>
    /// 收款方式字段表用来增加收款方式时生成输入项的配置表
    /// </summary>
    [Table("lt_paymentfield")]
    public class paymentfieldEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 支付方式id
        /// </summary>
        public int pid { get; set; }
        /// <summary>
        /// 输入字段名称 用作提交字段名
        /// </summary>
        public string fieldname { get; set; }
        /// <summary>
        /// 输入字段标题
        /// </summary>
        public string fieldtitle { get; set; }
        /// <summary>
        /// 输入字段提示
        /// </summary>
        public string fieldtip { get; set; }

        /// <summary>
        /// 输入图片标志 1=需要图片
        /// </summary>
        public int imgflag { get; set; }
        /// <summary>
        /// 数字标志
        /// </summary>
        public int digflag { get; set; }
        /// <summary>
        /// 排列顺序
        /// </summary>
        public int fieldsort { get; set; }
        /// <summary>
        /// 语言版本
        /// </summary>
        public string lang { get; set; }
    }
    [Table("lt_config")]
    public class configEntity
    {
        [Key]
        public int id { get; set; }
        public string config_key { get; set; }
        public int int_value { get; set; }
        public double d_value { get; set; }
        public string str_value { get; set; }
    }
    /// <summary>
    /// 兑换商操作取消记录
    /// </summary>
    [Table("lt_cancelrecord")]
    public class cancelrecordEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        public string ordersn { get; set; }
        public long c_time { get; set; }
        public long times { get; set; }
    }
}
