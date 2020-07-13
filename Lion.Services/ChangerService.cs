using DataCommon;
using Lion.Entity;
using Microsoft.Extensions.Configuration;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.Services
{
    public class ChangerService
    {
        private RpcNotifyService _rpcService;
        private IConfiguration _config;
        public ChangerService(RpcNotifyService rpcService,IConfiguration config)
        {
            _rpcService = rpcService;
            _config = config;
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
            var url = _config.GetValue<string>("OrderConfig:changerUrl");
            var ret = _rpcService.ChangerUnifiedorder(new ViewModel.requestModel.reqChangerUnifiedorder
            {
                amount = amount,
                currency_code = currency.code,
                order_type = 1,
                out_trade_no = guid,
                usn = usn,
                uphone=phone
            }, url);
            if (ret == null)
                throw new LionException("retnull", 1401);
            DataService.Insert<marketbuyrecordEntity>(new marketbuyrecordEntity
            {
                orderid=guid,
                orderno=ret.order_no,
                prepaysn=ret.prepay_id,
                currencyid=currency.id,
                amount=amount,
                uid=uid,
                times=DateTime.Now.GetTimeUnixLocal(),
                flag=21
            });
            return ret.prepay_id;
        }
        public string InsertMarketSellRecord(int uid, string usn, string phone,double amount, currencyEntity currency)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var url = _config.GetValue<string>("OrderConfig:changerUrl");
            var ret = _rpcService.ChangerUnifiedorder(new ViewModel.requestModel.reqChangerUnifiedorder
            {
                amount = amount,
                currency_code = currency.code,
                order_type = 2,
                out_trade_no = guid,
                usn = usn,
                uphone=phone
            }, url);
            if (ret == null)
                throw new LionException("retnull", 1401);
            DataService.Transcation(tc =>
            {
                int count=tc.CommandSet<marketsellrecordEntity>().Insert(new marketsellrecordEntity
                {
                    orderid = guid,
                    orderno = ret.order_no,
                    prepaysn = ret.prepay_id,
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
            return ret.prepay_id;
        }
        public marketbuyrecordEntity GetMarketBuyByOrderId(string orderid)
        {
            return DataService.Get<marketbuyrecordEntity>(o => o.orderid == orderid);
        }
        public marketsellrecordEntity GetMarketSellByOrderId(string orderid)
        {
            return DataService.Get<marketsellrecordEntity>(o => o.orderid == orderid);
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
    }
}
