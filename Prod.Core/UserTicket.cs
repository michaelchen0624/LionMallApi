using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core
{
    
    public class LoginTicket
    {
        public UserTicket userTicket { get; set; }
        public bool needRecommend { get; set; }
    }
    public class UserTicket
    {
        public string userId { get; set; }
        public string timeStamp { get; set; }
        public string ticketCode { get; set; }

        static readonly string _PrivateKey = "jhfdijogjhkj78438uv5n894875gvn98435bv609569097nbm9ecplfdjgk fdjogv34u9834vn8743sdf";
        public static bool CheckTicket(UserTicket ut)
        {
#if DEBUG
            if (true && ut.userId == "a5a9abfa86fd46ca893dd01255716e69")
            { //TODO:方便测试，发布前删除

                return true;
            }
#endif
            var time_stamp = ut.timeStamp;
            if (CreatTicket(ut.userId, time_stamp).ticketCode == ut.ticketCode)
            {
                return true;
            }
            return false;
        }

        public static UserTicket CreatTicket(string userId)
        {

            var time_stamp = DateHelper.GetTimeUnixLocal(DateTime.Now).ToString();

            return CreatTicket(userId, time_stamp);
        }

        public static UserTicket CreatTicket(string userId, string dt)
        {
            //加密因子:userId,内部密钥
            string ticketCode = "";
            string time_stamp = dt;

            string input = userId + time_stamp + _PrivateKey;

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                ticketCode = strResult.Replace("-", "");
            }

            return new UserTicket { userId = userId, timeStamp = time_stamp, ticketCode = ticketCode };

        }
    }
}
