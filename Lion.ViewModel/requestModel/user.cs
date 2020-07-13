using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lion.ViewModel.requestModel
{
    public class userRecommend
    {
        public string recommend { get; set; }
    }
    public class userData
    {
        public string userSN { set; get; }
        public string phone { get; set; }
        public string phonearea { get; set; }
        public string username { get; set; }
        public string passwd { get; set; }
        public string recommend { get; set; }
    }
    public class GiftData
    {
        public string phone { get; set; }
        public string phoneArea { get; set; }
        public string amount { get; set; }
        public string sign { get; set; }
    }
    public class LoginData
    {
        public string phone { get; set; }
        public string phoneArea { get; set; }
        public string msgCode { get; set; }
        public string msgSign { get; set; }
        public string pwd { get; set; }
        public string recommend { get; set; }
    }
    public class msgPhoneData
    {
        /// <summary>
        /// 电话号码
        /// </summary>
        [Required(ErrorMessage = "电话号码不能为空")]
        public string phone { get; set; }
        /// <summary>
        /// 电话区域号
        /// </summary>
        public string phonearea { get; set; }
    }
    public class GeneralRegData
    {
        [Required(ErrorMessage = "电话区域号不能空")]
        public string phonearea { get; set; }
        [Required(ErrorMessage = "电话号码不能为空")]
        public string phone { get; set; }
        public string recommend { get; set; }
        [Required(ErrorMessage = "验证码不能为空")]
        public string msgcode { get; set; }
    }
    public class RegData
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空")]
        public string username { get; set; }

        [Required(ErrorMessage = "电话区域号不能空")]
        public string phonearea { get; set; }
        [Required(ErrorMessage = "电话号码不能为空")]
        public string phone { get; set; }
        [Required(ErrorMessage = "密码不能为空")]
        public string paypwd { get; set; }
        public string recommend { get; set; }
        [Required(ErrorMessage = "验证码不能为空")]
        public string msgcode { get; set; }
    }
    public class reqTransfer
    {
        public string phone { get; set; }
        public string phonearea { get; set; }
        public double amount { get; set; }
    }
    public class reqRefillData
    {
        public double amount { get; set; }
    }
    public class reqUsn
    {
        public string usn { get; set; }
    }
    public class reqUserInfo
    {
        public string userName { get; set; }
        public string iconUrl { get; set; }
    }
    public class reqPage
    {
        public int pageIndex { get; set; }
    }
    public class upImgTypeData
    {
        public UpImgType imgType { get; set; } 
    }
    public class upImgAuth
    {
        public int imgType { get; set; }
        public string sign { get; set; }
    }
    public class upConfigItem
    {
        public string UserId { get; set; }
        public string contentType { get; set; }
        public long MaxContent { get; set; }
        public string secureKey { get; set; }
    }
    public enum UpImgType
    {
        headImg=2,
        qrCode=3,
        productImg=4
    }
    public class submitUpAuth
    {
        public string userId { get; set; }
        public string nonce_str { get; set; }
        public long expireTime { get; set; }
        public string sign { get; set; }
    }
    public class flashData
    {
        public int rate { get; set; }
    }
    public class avgBuyData
    {
        public double amount { get; set; }
    }

    /// <summary>
    /// 账单请求数据
    /// </summary>
    public class reqOrderFlowData
    {
        public int plat { get; set; }
        [Required]
        public string usn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LionPType p_type { get; set; }
        public double amount { get; set; }
        public double camount { get; set; }
        public string out_trade_no { get; set; }
        public string order_no { get; set; }
        public int currency { get; set; }
        public double rate { get; set; }
        public double fee { get; set; }
        public string sender { get; set; }
        public string arrive { get; set; }
        public int status { get; set; }
        public string extend { get; set; }
        public long o_time { get; set; }
        [Required]
        public string sign { get; set; }
    }
    public class phoneData
    {
        public string phone { get; set; }
        public string phonearea { get; set; }
    }
}
