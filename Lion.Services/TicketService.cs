using Lion.ViewModel.requestModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Prod.Core;
using Newtonsoft.Json;

namespace Lion.Services
{
    public class TicketService
    {
        private IConfiguration _config;
        public TicketService(IConfiguration config)
        {
            _config = config;
        }
        public string GetUpTicket(UpImgType data)
        {
            var upconfigList = _config.GetSection("UpConfig").Get<List<upConfigItem>>();
            var aesKey = _config.GetValue<string>("AESConfig:Key");
            var upconfig = upconfigList.Where(o => o.contentType.Equals(data.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (upconfig == null)
                throw new LionException("Up Type error", 1401);

            long expire = 0;
            var now = DateTime.Now;
            if (data == UpImgType.productImg)
                expire = now.AddMinutes(30).GetTimeUnixLocal();
            else
                expire = now.AddMinutes(2).GetTimeUnixLocal();

            var submit = new submitUpAuth
            {
                userId = upconfig.UserId,
                nonce_str = Guid.NewGuid().ToString().Replace("-", ""),
                expireTime = expire,
            };
            var paydata = new PayData<submitUpAuth>(submit);
            var sign = paydata.MakeSign(PaySignType.MD5, upconfig.secureKey);
            submit.sign = sign;
            var submitstr = JsonConvert.SerializeObject(submit);
            submitstr = SecureHelper.AESEncrypt(submitstr, aesKey);
            submitstr = SecureHelper.Base64Encode(submitstr);
            return submitstr;
        }
    }
}
