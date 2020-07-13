using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lion.ViewModel.requestModel
{
    public class payItem
    {
        /// <summary>
        /// 平台识别码
        /// </summary>
        public string platFormCode { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public string orderId { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public double amount { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

    }
    public class setInviteData
    {
        public string usn { get; set; }
        public string inviteCode { get; set; }
    }
    public class setPwdData
    {
        [Required(ErrorMessage ="密码不能为空")]
        public string passwd { get; set; }
        [Required(ErrorMessage ="验证码不能为空")]
        public string msgcode { get; set; }
        public string msgsign { get; set; }
        public string phone { get; set; }
        public string phonearea { get; set; }
    }
}
