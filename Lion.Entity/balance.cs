using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lion.Entity
{
    [Table("lt_exchangerate")]
    public class exchangerateEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 资产id
        /// </summary>
        public int asset_code { get; set; }
        /// <summary>
        /// 资产名称 usdt
        /// </summary>
        public string asset_name { get; set; }
        /// <summary>
        /// 货币id
        /// </summary>
        public int currency_id { get; set; }
        /// <summary>
        /// 货币代码 CNY
        /// </summary>
        public string currency_code { get; set; }
        /// <summary>
        /// 比率 1个资产=2 rm
        /// </summary>
        public double rate { get; set; }
        public long times { get; set; }
    }
    /// <summary>
    /// 资产种类表
    /// </summary>
    [Table("lt_integraltype")]
    public class integraltypeEntity
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// 资产名称
        /// </summary>
        public string c_name { get; set; }
        public string c_unit { get; set; }
        /// <summary>
        /// 主要资产的标识
        /// </summary>
        public int main_flag { get; set; }
        /// <summary>
        /// 资产分类 1=btc类型 2=以太坊类型
        /// </summary>
        public int coin_type { get; set; }
        /// <summary>
        /// 最小出金金额
        /// </summary>
        public double withdraw_min { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double withdraw_fee { get; set; }
        public double usersell_max { get; set; }
        public double usersell_min { get; set; }
        /// <summary>
        /// 对充值货币的比率 1usdt= 7.01ubnk
        /// </summary>
        public double ratio { get; set; }
        /// <summary>
        /// 删除标志
        /// </summary>
        public int del { get; set; }
    }
    [Table("lt_userbalance")]
    public class userbalanceEntity
    {
        public int uid { get; set; }
        public string usn { get; set; }
        /// <summary>
        /// 资产代码
        /// </summary>
        public int asset_code { get; set; }
        /// <summary>
        /// 资产名称(用于展示)
        /// </summary>
        public string asset_name { get; set; }
        /// <summary>
        /// 累计充值
        /// </summary>
        public double recharge_total { get; set; }
        /// <summary>
        /// 购物余额(usd)
        /// </summary>
        public double balance_shopping { get; set; }
        /// <summary>
        /// 奖励余额(usd)
        /// </summary>
        public double balance_reward { get; set; }
        /// <summary>
        /// 购物冻结(usd)
        /// </summary>
        public double frozen_shopping { get; set; }
        /// <summary>
        /// 奖励冻结(usd)
        /// </summary>
        public double frozen_reward { get; set; }
        ///// <summary>
        ///// 地址
        ///// </summary>
        //public string address { get; set; }
        //public string public_key { get; set; }
        public string private_key { get; set; }
    }
    /// <summary>
    /// 预付单,操作余额前生成
    /// </summary>
    [Table("lt_prepayorder")]
    public class prepayorderEntity
    {
        [Key]
        public int pid { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string psn { get; set; }
        /// <summary>
        /// 用户guid
        /// </summary>
        public string usn { get; set; }
        /// <summary>
        /// 单据类型 prepayType类型
        /// </summary>
        public int p_type { get; set; }
        /// <summary>
        /// 外部单号
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public int asset_code { get; set; }
        /// <summary>
        /// 资产金额
        /// </summary>
        public double amount { get; set; }
        /// <summary>
        /// 货币编号
        /// </summary>
        public int currency_id { get; set; }
        /// <summary>
        /// 货币金额
        /// </summary>
        public double camount { get; set; }
        /// <summary>
        /// 汇率 1usdt= currency
        /// </summary>
        public double rate { get; set; }
        public int flag { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public long begin_time { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public long end_time { get; set; }
        public string extend { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long times { get; set; }
    }
    [Table("lt_rechargeaddr")]
    public class rechargeaddrEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        public int asset_code { get; set; }
        /// <summary>
        /// 1=btc网络 2=eth网络
        /// </summary>
        public int coin_type { get; set; }
        public string addr { get; set; }
        public string public_key { get; set; }
        public string private_key { get; set; }
    }
    /// <summary>
    /// 充值列表
    /// </summary>
    [Table("lt_rechargerecord")]
    public class rechargerecordEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        public string usn { get; set; }
        /// <summary>
        /// 外部单号 场外购买单就是marketbuy的单号
        /// </summary>
        public string out_trade_no { get; set; }
        public string txid { get; set; }
        public int asset_code { get; set; }
        public int sender { get; set; }
        /// <summary>
        /// 批次号 2019-10-18
        /// </summary>
        public string batch { get; set; }
        public int recharge_type { get; set; }
        public double amount { get; set; }
        public string c_name { get; set; }
        public double c_amount { get; set; }
        public long times { get; set; }
    }
    [Table("lt_wallet")]
    public class walletEntity
    {
        public int uid { get; set; }
        public string usn { get; set; }
        /// <summary>
        /// gcoin余额
        /// </summary>
        public double balance { get; set; }
        /// <summary>
        /// 昨个奖励
        /// </summary>
        public double last_reward { get; set; }
        /// <summary>
        /// 累计奖励
        /// </summary>
        public double total_reward { get; set; }
        /// <summary>
        /// 奖励折合 us
        /// </summary>
        public double total_reward_us { get; set; }
        /// <summary>
        /// 快兑比率 整数
        /// </summary>
        public int flash_rate { get; set; }
    }
    /// <summary>
    /// 回购记录表
    /// </summary>
    [Table("lt_platbuyrecord")]
    public class platbuyrecordEntity
    {
        [Key]
        public int id { get; set; }
        public int uid { get; set; }
        /// <summary>
        /// gcoin数量
        /// </summary>
        public double amount { get; set; }
        /// <summary>
        /// 兑换成usdt的数量
        /// </summary>
        public double conver_us { get; set; }
        /// <summary>
        /// 回购类型 1=闪兑回购 2=均价回购
        /// </summary>
        public int b_type { get; set; }
        /// <summary>
        /// 价格 0.1 usdt/gcoin
        /// </summary>
        public double price { get; set; }
        /// <summary>
        /// 批次号 闪兑回购的结算号
        /// </summary>
        public string batch { get; set; }
        public long times { get; set; }
    }
    /// <summary>
    /// 价格记录表
    /// </summary>
    [Table("lt_pricerecord")]
    public class pricerecordEntity
    {
        /// <summary>
        /// 批次号 2019-09-06
        /// </summary>
        public string batch { get; set; }
        /// <summary>
        /// 发行价
        /// </summary>
        public double issue_price { get; set; }
        /// <summary>
        /// 回购价
        /// </summary>
        public double buy_price { get; set; }
        public long times { get; set; }
    }
    [Table("lt_settlerecord")]
    public class settlerecordEntity
    {
        /// <summary>
        /// 结算日期
        /// </summary>
        public string settle_date { get; set; }
        /// <summary>
        /// 当期充值
        /// </summary>
        public double total_recharge { get; set; }
        /// <summary>
        /// 当期总贡献值
        /// </summary>
        public double total_contribu { get; set; }
        /// <summary>
        /// 当期发行量
        /// </summary>
        public double total_issue { get; set; }
        /// <summary>
        /// 发行价
        /// </summary>
        public double price { get; set; }
        /// <summary>
        /// 当期闪兑回购量 gcoin
        /// </summary>
        public double total_flash { get; set; }
        /// <summary>
        /// 当期闪兑回购金额
        /// </summary>
        public double flash_us { get; set; }
        /// <summary>
        /// 当期均价回购量 gcoin
        /// </summary>
        public double total_avg { get; set; }
        /// <summary>
        /// 当期均价回购金额
        /// </summary>
        public double avg_us { get; set; }
        public int handle_id { get; set; }
        public int flag { get; set; }
        public long begin_time { get; set; }
        public long end_time { get; set; }
        public long times { get; set; }
    }
    [Table("lt_platdata")]
    public class platdataEntity
    {
        public int pid { get; set; }
        /// <summary>
        /// 累计消费
        /// </summary>
        public double total_recharge { get; set; }
        /// <summary>
        /// 累计贡献值
        /// </summary>
        public double total_contribu { get; set; }
        /// <summary>
        /// 累计发行量gcoin
        /// </summary>
        public double total_issue { get; set; }
        /// <summary>
        /// 累计闪兑gcoin
        /// </summary>
        public double total_flash { get; set; }
        /// <summary>
        /// 累计闪兑金额
        /// </summary>
        public double flash_us { get; set; }
        /// <summary>
        /// 累计均价回购gcoin
        /// </summary>
        public double total_avg { get; set; }
        /// <summary>
        /// 累计均价回购金额
        /// </summary>
        public double avg_us { get; set; }
    }
    /// <summary>
    /// 充值类型
    /// </summary>
    public enum rechargeType
    {
        General=1,//普通充值 区块充值
        Market=2,//场外购买
        Transfer=3,//站内转入
        Reward=4//奖励余额转入
    }
}
