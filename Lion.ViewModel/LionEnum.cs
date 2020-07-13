using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.ViewModel
{
    public enum orderNotifyStatus
    {
        success = 1,
        cancel = -1
    }
    public enum LionPType
    {
        普通充值 = 1,
        普通提现 = 2,
        场外充值 = 3,
        场外提现 = 4,
        站内转入 = 5,
        站内转出 = 6,
        站内充值 = 7,
        购物消费 = 8,
        闪兑回购 = 9,
        均价回购 = 10,
        购物退款 = 12
    }
    public enum MsgTemplate
    {
        CashCode=1,//提现
        ValidCode=2,//验证码
        OrderNotice=3 //订单提示
    }

}
