using Newtonsoft.Json;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionMall.Tools
{
    public class MsgTool
    {
        private IHttpUtils _httpUtils;
        public MsgTool(IHttpUtils httpUtils)
        {
            _httpUtils = httpUtils;
        }
        public TResp SendMsg<TResp>(msgContent content)
        {
            var msgcontent = new {
                account=content.account,
                password=content.password,
                mobile=content.mobile,
                msg=content.msg
            };
            var response = _httpUtils.httpPost(content.url, JsonConvert.SerializeObject(msgcontent),
                "application/json");
            return JsonConvert.DeserializeObject<TResp>(response);
        }
    }
    public class msgContent
    {
        public string account { get; set; }
        public string password { get; set; }
        public string msg { get; set; }
        public string mobile { get; set; }
        public string url { get; set; }
    }
    /// <summary>
    /// 发送短信接收内容
    /// </summary>
    public class clmsgResp
    {
        public string code { get; set; }
        public string error { get; set; }
        public string balance { get; set; }
    }
    public enum clmsgCode
    {
        提交成功 = 0,
        账号不存在 = 101,
        密码错误 = 102,
        短信内容长度错误 = 106,
        手机号码格式错误 = 108,
        余额不足 = 110,
        产品配置错误 = 112,
        请求ip和绑定ip不一致 = 114,
        没有开通国内短信权限 = 115,
        短信内容不能为空 = 123,
        账号长度错误 = 128,
        产品价格配置错误 = 129
    }
}
