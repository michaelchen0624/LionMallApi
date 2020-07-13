using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.Entity
{
    /// <summary>
    /// 预付单结果 对接各种tmp表 =null 单据不存在flag=0, flag=1 已存在并可用 =21订单已关闭 =22订单已完成
    /// </summary>
    public class prepayRet
    {
        public int assetId { get; set; }
        /// <summary>
        /// 对应货币id
        /// </summary>
        public int currencyId { get; set; }
        /// <summary>
        /// 资产金额
        /// </summary>
        public double amount { get; set; }
        /// <summary>
        /// 对应币种金额
        /// </summary>
        public double camount { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        public double rate { get; set; }
        /// <summary>
        /// 标志
        /// </summary>
        public int flag { get; set; }
        public double fee { get; set; }
        public string receiveSn { get; set; }
    }
    public enum prepayType
    {
        Shopping = 1,//余额购买
        Withdrawal = 2,//普通提现
        MarketSell = 3,//场外出售
        Transfer = 4,//站内互转
        Reward=5,//奖励余额转购物余额
        PlatBuy=12 //平台回购
    }
    
}
