using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extension.Core
{
    public class DapperExtensionException:ApplicationException
    {
        public DapperExtensionException(string msg):base(msg)
        {

        }
    }
}
