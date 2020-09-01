using DataCommon;
using Lion.Entity;
using Microsoft.Extensions.Configuration;
using Prod.Core;
using System;
using System.Collections.Generic;
using Lion.ViewModel;
using Lion.ViewModel.requestModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lion.Services
{
    public class ChangerService
    {
        private RpcNotifyService _rpcService;
        private IConfiguration _config;
        private IHttpUtils _httpUtils;
        public ChangerService(RpcNotifyService rpcService,IConfiguration config, IHttpUtils httpUtils)
        {
            _rpcService = rpcService;
            _config = config;
            _httpUtils = httpUtils;
        }
        public marketbuyrecordEntity GetPendingMarketBuyRecordByUid(int uid)
        {
            var record = DataService.Get<marketbuyrecordEntity, int>(o => o.flag ==21 && o.uid == uid,o=>o.id,Dapper.Extension.Core.EOrderBy.Desc);
            return record;
        }
        public marketsellrecordEntity GetPendingMarketSellRecordByUid(int uid)
        {
            var record = DataService.Get<marketsellrecordEntity, int>(o => o.flag == 21 && o.uid == uid, o => o.id, Dapper.Extension.Core.EOrderBy.Desc);
            return record;
        }
        public string InsertMarketBuyRecord(int uid,string usn,string phone,double amount,currencyEntity currency)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            //var url = _config.GetValue<string>("OrderConfig:changerUrl");
            //var ret = _rpcService.ChangerUnifiedorder(new ViewModel.requestModel.reqChangerUnifiedorder
            //{
            //    amount = amount,
            //    currency_code = currency.code,
            //    order_type = 1,
            //    out_trade_no = guid,
            //    usn = usn,
            //    uphone = phone
            //}, url);
            //if (ret == null)
            //    throw new LionException("retnull", 1401);
            DataService.Insert<marketbuyrecordEntity>(new marketbuyrecordEntity
            {
                orderid=guid,
                //orderno=ret.order_no,
                //prepaysn=ret.prepay_id,
                currencyid=currency.id,
                amount=amount,
                uid=uid,
                times=DateTime.Now.GetTimeUnixLocal(),
                flag=21
            });
            return guid;
        }
        public string InsertMarketSellRecord(int uid, string usn, string phone,double amount, currencyEntity currency)
        {
            reqChainWithdrawData data = new reqChainWithdrawData() 
            {
            
            };
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var url = _config.GetValue<string>("SendChain:withdrawUrl");
            var ret = _httpUtils.httpPost(url, JsonConvert.SerializeObject(data), "application/json");
            //var url = _config.GetValue<string>("OrderConfig:changerUrl");
            //var ret = _rpcService.ChangerUnifiedorder(new ViewModel.requestModel.reqChangerUnifiedorder
            //{
            //    amount = amount,
            //    currency_code = currency.code,
            //    order_type = 2,
            //    out_trade_no = guid,
            //    usn = usn,
            //    uphone = phone
            //}, url);
            if (ret == null)
                throw new LionException("retnull", 1401);
            DataService.Transcation(tc =>
            {
                int count=tc.CommandSet<marketsellrecordEntity>().Insert(new marketsellrecordEntity
                {
                    orderid = guid,
                    //orderno = ret.order_no,
                    //prepaysn = ret.prepay_id,
                    currencyid = currency.id,
                    amount = amount,
                    uid = uid,
                    times = DateTime.Now.GetTimeUnixLocal(),
                    flag = 21
                });
                int count1=tc.CommandSet<userbalanceEntity>().Where(o => o.uid == uid).Incre(o => new userbalanceEntity
                {
                    //balance_reward=-amount,
                    frozen_reward=amount
                }).UpdateOnlyIncre();
                if (count <= 0 || count1 <= 0)
                    throw new PayException("处理失败", 404);
            });
            return guid;
        }
        public marketbuyrecordEntity GetMarketBuyByOrderId(string orderid)
        {
            return DataService.Get<marketbuyrecordEntity>(o => o.orderid == orderid);
        }
        public marketsellrecordEntity GetMarketSellByOrderId(string orderid)
        {
            return DataService.Get<marketsellrecordEntity>(o => o.orderid == orderid);
        }
        public marketbuyrecordEntity GetMarketBuyByOrderNo(string orderno)
        {
            return DataService.Get<marketbuyrecordEntity>(o => o.orderno == orderno);
        }
        public marketsellrecordEntity GetMarketSellByOrderNo(string orderno)
        {
            return DataService.Get<marketsellrecordEntity>(o => o.orderno == orderno);
        }
        /// <summary>
        /// 当前累计取消数量
        /// </summary>
        /// <param name="csn"></param>
        /// <returns></returns>
        public int CancelCountOfCurrent(int uid)
        {
            var now = DateTime.Now;
            var start = now.GetFirstOfCurrent();
            var end = now.GetLastOfCurrent();
            var count = DataService.GetCount<usercancelEntity>(o => o.times >= start && o.times < end &&
            o.uid ==uid);
            return count;
        }

        public RspTradeHis GetTradeList(RqTradeHis his)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                var rsp = new RspTradeHis();
                if (his.Type == 0)
                {
                    strSql.AppendLine("SELECT COUNT(1) FROM (SELECT * FROM [lt_marketsellrecord] union all SELECT * FROM [lt_marketbuyrecord]) t1 JOIN [lt_user] t2 on t1.uid = t2.uid WHERE (1 = 1");
                    if (!string.IsNullOrEmpty(his.userName))
                    {
                        strSql.Append(" AND t2.username like @username");
                    }
                    strSql.Append(" AND t1.times >= @beginDate AND t1.times <= @endDate");
                    strSql.Append(");");
                   rsp.count = DataService.GetCount(strSql.ToString(), new { username = his.userName, beginDate = Convert.ToDateTime(his.begindate).GetTimeUnixLocal(), endDate = Convert.ToDateTime(his.enddate).GetTimeUnixLocal() });

                    strSql.AppendLine($"SELECT TOP {his.pagelimit} * FROM (SELECT ROW_NUMBER() OVER(ORDER BY orderid DESC) AS ROWNUMBER,t3.* FROM (SELECT *,'提现' as type FROM [lt_marketsellrecord] union all SELECT *,'充值' as type FROM [lt_marketbuyrecord]) t3 JOIN [lt_user] t4 on t3.uid = t4.uid WHERE(1 = 1");
                    if (!string.IsNullOrEmpty(his.userName))
                    {
                        strSql.Append(" AND t4.username like CONCAT('%',@username,'%')");
                    }
                    strSql.Append(" AND t3.times >= @beginDate AND t3.times <= @endDate)) T");
                    strSql.AppendLine($" WHERE ROWNUMBER > {(his.page - 1) * his.pagelimit} AND ROWNUMBER <= {his.page * his.pagelimit} ORDER BY times DESC;");
                    var buyList = DataService.QueryPageListBySql<TradeRecord>(strSql.ToString(), new { username = his.userName, beginDate = Convert.ToDateTime(his.begindate).GetTimeUnixLocal(), endDate = Convert.ToDateTime(his.enddate).GetTimeUnixLocal() }, his.page, his.pagelimit).ToList();
                    rsp.list = buyList.Select(o => new TradeHis()
                    {
                        orderid = o.orderid,
                        orderno = o.orderno,
                        prepaysn = o.prepaysn,
                        username = DataService.Get<userEntity>(u => u.uid == o.uid).username,
                        amount = o.amount,
                        currency = "CNY",
                        rate = o.rate,
                        camount = o.camount,
                        fee = o.fee,
                        confirmtime =  o.confirmtime,
                        time = o.times,
                        type = o.type,
                        state = Enum.GetName(typeof(EnumSet.OrderStatus), o.flag)
                    }).ToList();

                }
                else if (his.Type == 1)
                {
                    strSql.AppendLine("SELECT COUNT(1) FROM [lt_marketbuyrecord] t1 JOIN [lt_user] t2 on t1.uid = t2.uid WHERE (1 = 1");
                    if (!string.IsNullOrEmpty(his.userName))
                    {
                        strSql.Append(" AND t2.username like @username");
                    }
                    strSql.Append(" AND t1.times >= @beginDate AND t1.times <= @endDate");
                    strSql.Append(");");
                    rsp.count = DataService.GetCount(strSql.ToString(), new { username = his.userName, beginDate = Convert.ToDateTime(his.begindate).GetTimeUnixLocal(), endDate = Convert.ToDateTime(his.enddate).GetTimeUnixLocal() });

                    strSql.AppendLine($"SELECT TOP {his.pagelimit} * FROM (SELECT ROW_NUMBER() OVER(ORDER BY orderid DESC) AS ROWNUMBER,t3.* FROM [lt_marketbuyrecord] t3 JOIN [lt_user] t4 on t3.uid = t4.uid WHERE(1 = 1");
                    if (!string.IsNullOrEmpty(his.userName))
                    {
                        strSql.Append(" AND t4.username like CONCAT('%',@username,'%')");
                    }
                    strSql.Append(" AND t3.times >= @beginDate AND t3.times <= @endDate)) T");
                    strSql.AppendLine($" WHERE ROWNUMBER > {(his.page - 1) * his.pagelimit} AND ROWNUMBER <= {his.page * his.pagelimit} ORDER BY times DESC;");
                    var buyList = DataService.QueryPageListBySql<marketbuyrecordEntity>(strSql.ToString(), new { username = his.userName, beginDate = Convert.ToDateTime(his.begindate).GetTimeUnixLocal(), endDate = Convert.ToDateTime(his.enddate).GetTimeUnixLocal() }, his.page, his.pagelimit).ToList();
                    rsp.list = buyList.Select(o => new TradeHis()
                    {
                        orderid = o.orderid,
                        orderno = o.orderno,
                        prepaysn = o.prepaysn,
                        username = DataService.Get<userEntity>(u => u.uid == o.uid).username,
                        amount = o.amount,
                        currency = "CNY",
                        rate = o.rate,
                        camount = o.camount,
                        fee = o.fee,
                        confirmtime = o.confirmtime,
                        time = o.times,
                        type = "充值",
                        state = Enum.GetName(typeof(EnumSet.OrderStatus), o.flag)
                    }).ToList();
                }
                else if (his.Type == 2)
                {
                    strSql.AppendLine("SELECT COUNT(1) FROM [lt_marketsellrecord] t1 JOIN [lt_user] t2 on t1.uid = t2.uid WHERE (1 = 1");
                    if (!string.IsNullOrEmpty(his.userName))
                    {
                        strSql.Append(" AND t2.username like @username");
                    }
                    strSql.Append(" AND t1.times >= @beginDate AND t1.times <= @endDate");
                    strSql.Append(");");
                    rsp.count = DataService.GetCount(strSql.ToString(), new { username = his.userName, beginDate = Convert.ToDateTime(his.begindate).GetTimeUnixLocal(), endDate = Convert.ToDateTime(his.enddate).GetTimeUnixLocal() });

                    strSql.AppendLine($"SELECT TOP {his.pagelimit} * FROM (SELECT ROW_NUMBER() OVER(ORDER BY orderid DESC) AS ROWNUMBER,t3.* FROM [lt_marketsellrecord] t3 JOIN [lt_user] t4 on t3.uid = t4.uid WHERE(1 = 1");
                    if (!string.IsNullOrEmpty(his.userName))
                    {
                        strSql.Append(" AND t4.username like CONCAT('%',@username,'%')");
                    }
                    strSql.Append(" AND t3.times >= @beginDate AND t3.times <= @endDate)) T");
                    strSql.AppendLine($" WHERE ROWNUMBER > {(his.page - 1) * his.pagelimit} AND ROWNUMBER <= {his.page * his.pagelimit} ORDER BY times DESC;");
                    var buyList = DataService.QueryPageListBySql<marketsellrecordEntity>(strSql.ToString(), new { username = his.userName, beginDate = Convert.ToDateTime(his.begindate).GetTimeUnixLocal(), endDate = Convert.ToDateTime(his.enddate).GetTimeUnixLocal() }, his.page, his.pagelimit).ToList();
                    rsp.list = buyList.Select(o => new TradeHis()
                    {
                        orderid = o.orderid,
                        orderno = o.orderno,
                        prepaysn = o.prepaysn,
                        username = DataService.Get<userEntity>(u => u.uid == o.uid).username,
                        amount = o.amount,
                        currency = "CNY",
                        rate = o.rate,
                        camount = o.camount,
                        fee = o.fee,
                        confirmtime = o.confirmtime,
                        time = o.times,
                        type = "提现",
                        state = Enum.GetName(typeof(EnumSet.OrderStatus), o.flag)
                    }).ToList();
                }
                return rsp;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
