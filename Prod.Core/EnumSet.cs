using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core
{
    public class EnumSet
    {
        public enum OrderStatus
        {
            已完成 = 1,
            待确认 = 21,
            已取消 = -1
        }
    }  
}
