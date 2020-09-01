using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prod.Core;
using LionMall.Tools;

namespace Lion.Services
{
    public class RpcNotifyService
    {
        private IHttpUtils _httpUtils;
        private ILogService _log;
        private IConfiguration _config;
        public RpcNotifyService(IHttpUtils httpUtils,ILogService log, IConfiguration config)
        {
            _httpUtils = httpUtils;
            _log = log;
            _config = config;
        }
        public string SendShopping(string url,string json)
        {
            //Thread.Sleep(6000);
            var ret = _httpUtils.httpPost(url, json, "application/json");
            //_log.I("ret", ret);
            return ret;
        }
        public respRetChangerPrepay ChangerUnifiedorder(reqChangerUnifiedorder data,string url)
        {
            var paydata = new PayData<reqChangerUnifiedorder>(data);
            data.sign = paydata.MakeSign(PaySignType.MD5, "1234");
            var ret = _httpUtils.httpPost(url, JsonConvert.SerializeObject(data), "application/json");
            JObject jsondata = JObject.Parse(ret);
            var result = jsondata["result"];
            respRetChangerPrepay prepay = null;
            if(jsondata["code"].ToString()=="200" && result.HasValues)
            {
                prepay = result.ToObject<respRetChangerPrepay>();
                if (prepay == null)
                    throw new PayException("prepayerror", 1401);
                var retpaydata = new PayData<respRetChangerPrepay>(prepay);
                if (retpaydata.MakeSign(PaySignType.MD5, "1234") != prepay.sign)
                    throw new PayException("signerror", 1401);
                return prepay;
            }
            return prepay;
        }
        /// <summary>
        /// 调用账单数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool FlowOrderData(reqOrderFlowData data)
        {
            data.plat = 1;
            var paydata = new PayData<reqOrderFlowData>(data);
            data.sign = paydata.MakeSign(PaySignType.MD5, "1234");
            var url = _config.GetValue<string>("OrderConfig:flowUrl");
            var ret = _httpUtils.httpPost(url, JsonConvert.SerializeObject(data), "application/json");
            JObject jsondata = JObject.Parse(ret);
            var result = jsondata["result"];
            bool prepay = false;
            if (jsondata["code"].ToString() == "200" && result!=null)
            {
                prepay = result.ToObject<bool>();
            }
            if (!prepay)
                _log.I("flowError", JsonConvert.SerializeObject(data));
            return prepay;
        }
        /// <summary>
        /// 调用短信接口
        /// </summary>
        /// <returns></returns>
        public string SendMsg(reqMsgData data)
        {
            var msgData = new PayData<reqMsgData>(data);
            data.sign = msgData.MakeSign(PaySignType.MD5, "1234");
            var url = _config.GetValue<string>("OrderConfig:msgUrl");
            var ret = _httpUtils.httpPost(url, JsonConvert.SerializeObject(data), "application/json");
            JObject jsondata = JObject.Parse(ret);
            var result = jsondata["result"];          
            if (jsondata["code"].ToString() == "200" && result.HasValues)
            {
                var retCode = result["code"];
                if (retCode!=null && result["code"].ToString() == "0")
                    return "success";
                else
                {
                    var retMsg = result["msg"];
                    _log.E("SendMsg", ret);
                    return retMsg == null ? "" : retMsg.ToString();
                }
            }
            else
            {
                var obj = result["msg"];
                _log.E("SendMsg", ret);
                return obj==null?"发送不成功":obj.ToString();
            }
            //if (!prepay)
            //    _log.I("flowError", JsonConvert.SerializeObject(data));
            ////return prepay;
            //return true;
        }
        public string GetNewAddr(reqChainData data)
        {
            var paydata = new PayData<reqChainData>(data);
            data.sign = paydata.MakeSign(PaySignType.MD5, "1234");
            var url = _config.GetValue<string>("SendChain:getAddrUrl");
            var prepay = _httpUtils.httpPost(url, JsonConvert.SerializeObject(data), "application/json");
            if (string.IsNullOrWhiteSpace(prepay) || !prepay.Contains("0x"))
                prepay = "";
            return prepay;
        }

        public string GetAddr(reqAddress usn)
        {
            var url = _config.GetValue<string>("SendChain:getAddrUrl");
            var ret = _httpUtils.httpPost(url, JsonConvert.SerializeObject(usn), "application/json");
            JObject obj = JObject.Parse(ret);
            if (!obj.ContainsKey("data"))
            {
                return "";
            }
            JObject data = JObject.Parse(obj["data"].ToString());
            return data["address"].ToString();
        }

        public object GetOutTradeHis(string address) 
        {
            var url = _config.GetValue<string>("OrderConfig:changerUrl");
            url += "?module=account&action=txlist&address=" + address + "&startblock=0&endblock=99999999&page=1&offset=10&sort=asc&apikey=BJXHID3AEKK5IH9YWP62HA9H942HZ3ATF2";
            var ret = _httpUtils.httpGet(url);
            return "";
        }
    }
}
