using DataCommon;
using Lion.Entity;
using Lion.ViewModel;
using Lion.ViewModel.requestModel;
using LionMall.Tools;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.Services
{
    public class CommonService
    {
        private MsgTool _msgtool;
        private IConfiguration _config;
        private ILogService _log;
        private ICacheService _cache;
        private RpcNotifyService _rpcService;
        public CommonService(MsgTool msgtool,IConfiguration config,ILogService log,ICacheService cache,RpcNotifyService rpcService)
        {
            _msgtool = msgtool;
            _config = config;
            _log = log;
            _cache = cache;
            _rpcService = rpcService;
        }
        public msgconfigEntity GetMsgConfigByActive()
        {
            return DataService.Get<msgconfigEntity>(o => o.active == 1);
        }
        public string SendMsg(string phone, int length, msgType msgtype, string phonearea = "86", MsgTemplate tpl=MsgTemplate.ValidCode,
            string lang = "en", bool phonekey = false,int expireSecond=300)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new LionException("电话号码不能为空", 404);
            var sendflag = _config.GetSection("SendMsg:sendflag").Value;
            string ret = string.Empty;
            var msgconfig = GetMsgConfigByActive();
            var msgCode = LionRandom.GenerateRandomCode(length);
            //string content = string.Empty;
            var phonestr = $"{phonearea}{phone}";
            //content = $"Your verification code is:{msgCode}";
            if (sendflag == "1")
            {
                var p_data = new reqMsgData
                {
                    AreaCode = phonearea,
                    Mobile = phone,
                    Msg = msgCode,
                    TemplateCode = tpl.ToString()
                };
                var flag= _rpcService.SendMsg(p_data);
                if (flag!="success")
                {
                    //_log.I("msg", $"{phonearea} {phone},发送不成功,errmsg:{flag} {JsonConvert.SerializeObject(p_data)}");
                    return $"errmsg:{flag}";
                }
            }
            var sign = phonekey ? phonestr : Guid.NewGuid().ToString().Replace("-", "");
            _cache.Set(sign, msgCode, expireSecond, false);
            return sign;


            //if (string.IsNullOrWhiteSpace(phone))
            //    throw new LionException("电话号码不能为空", 404);
            //var sendflag = _config.GetSection("SendMsg:sendflag").Value;
            //string ret = string.Empty;
            //var msgconfig = GetMsgConfigByActive();
            //var msgCode = LionRandom.GenerateRandomCode(length);
            //string content = string.Empty;
            //var phonestr = $"{phonearea}{phone}";
            //content = $"Your verification code is:{msgCode}";
            //if (sendflag == "1")
            //{
            //    var resp = _msgtool.SendMsg<clmsgResp>(new msgContent
            //    {
            //        account=msgconfig.account,
            //        mobile=phonestr,
            //        msg=content,
            //        password=msgconfig.passwd,
            //        url=msgconfig.url
            //    });
            //    if (resp.code != "0")
            //    {
            //        _log.I("msg", $"{phonearea} {phone},发送不成功,errmsg:{resp.error},errcode;{resp.code}");
            //        return $"errmsg:短信发送不成功";
            //    }
            //}
            //var sign = phonekey ? phonestr :Guid.NewGuid().ToString().Replace("-", "");
            //_cache.Set(sign, msgCode, expireSecond, false);
            //return sign;
        }
        public bool verifyMsg(string msgSign, string msgCode,int msgLength=4)
        {
            var sendflag = _config.GetValue<int>("SendMsg:sendflag");
            var code = _cache.Get<string>(msgSign);
            if (sendflag==0)//不发短信的情况
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;
                if(msgLength==6)
                    return msgCode == "123456";
                return msgCode == "1234";
            }
            return msgCode == code;
        }
        public string CutAmount(double amount, int number = 2)
        {
            var amountStr = amount.ToString();
            var postion = amountStr.IndexOf('.');
            if (postion < 0)
                return amountStr;
            var length = postion + number + 1 > amountStr.Length ? amountStr.Length : postion + number + 1;
            return amountStr.Substring(0, length);
        }
    }
    public enum msgType
    {
        Register = 1,//注册
        Login = 2,//登陆
        WithDraw = 3,//提现
        Modify = 4,//更改信息
        Exchange = 5, //站内互转
        Verify = 7
    }
}
