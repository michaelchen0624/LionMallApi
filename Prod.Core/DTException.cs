using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core
{
    public class LionException:Exception
    {
        public int Code { get; set; }
        public LionException(string message,int code):base(message)
        {
            this.Code = code;
        }
    }
    public class PayException : Exception
    {
        public int Code { get; set; }
        public PayException(string message, int code) : base(message)
        {
            this.Code = code;
        }
    }
}
