using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Prod.Core;

namespace Lion.Services
{
    public class PayData<T>
    {
        //public const string SIGN_TYPE_MD5 = "MD5";
        //public const string SIGN_TYPE_HMAC_SHA256 = "HMAC-SHA256";
        private SortedDictionary<string, string> m_values = new SortedDictionary<string, string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        public PayData(T t)
        {
            var properties = t.GetType().GetProperties();
            foreach (var propertiy in properties)
            {
                var value = propertiy.GetValue(t);
                if (value == null) continue;
                if (string.IsNullOrWhiteSpace(value.ToString()))
                    continue;
                m_values[propertiy.Name] = value.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, string value)
        {
            m_values[key] = value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToUrl()
        {
            var buff = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in m_values)
            {
                if (pair.Value == null)
                {
                    throw new PayException("ToUrl转换失败", 404);
                }

                if (pair.Key != "sign" && pair.Value.ToString() != "")
                {
                    //buff += pair.Key + "=" + pair.Value + "&";
                    buff.Append($"{pair.Key}={pair.Value}&");
                }
            }
            return buff.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsSet(string key)
        {
            string o = string.Empty;
            m_values.TryGetValue(key, out o);
            if (!string.IsNullOrWhiteSpace(o))
                return true;
            else
                return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue(string key)
        {
            string o = string.Empty;
            m_values.TryGetValue(key, out o);
            return o;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string MakeSign(PaySignType signType, string key)
        {
            //转url格式
            string str = ToUrl();
            //在string后加入API KEY
            str = $"{str}&key={key}";
            if (signType == PaySignType.MD5)
            {
                var md5 = MD5.Create();
                var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("X2"));
                }
                //所有字符转为大写
                return sb.ToString();
            }
            else if (signType == PaySignType.SHA256)
            {
                return CalcHMACSHA256Hash(str, key);
            }
            else
            {
                throw new PayException("sign_type 不合法", 404);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool CheckSign(PaySignType signType, string key)
        {
            //如果没有设置签名，则跳过检测
            if (!IsSet("sign"))
            {
                throw new PayException("签名不正确!", 404);
            }
            //获取接收到的签名
            string return_sign = GetValue("sign").ToString();

            //在本地计算新的签名
            string cal_sign = MakeSign(signType, key);

            if (cal_sign == return_sign)
            {
                return true;
            }
            throw new PayException("签名验证错误!", 404);
        }
        private string CalcHMACSHA256Hash(string plaintext, string salt)
        {
            string result = "";
            var enc = Encoding.Default;
            byte[]
            baText2BeHashed = enc.GetBytes(plaintext),
            baSalt = enc.GetBytes(salt);
            System.Security.Cryptography.HMACSHA256 hasher = new HMACSHA256(baSalt);
            byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            result = string.Join("", baHashedText.ToList().Select(b => b.ToString("x2")).ToArray());
            return result;
        }
    }
    public enum PaySignType
    {
        /// <summary>
        /// 
        /// </summary>
        MD5 = 1,
        /// <summary>
        /// 
        /// </summary>
        SHA256 = 2
    }
}
