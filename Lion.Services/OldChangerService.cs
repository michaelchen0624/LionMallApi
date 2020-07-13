using DataCommon;
using Lion.Entity;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Dapper.Extension.MsSql;

namespace Lion.Services
{
    public class OldChangerService
    {
        private ICacheService _cache;
        public OldChangerService(ICacheService cache)
        {
            _cache = cache;
        }
        public IEnumerable<currencyEntity> GetCurrencyList()
        {
            var list = _cache.Get<IEnumerable<currencyEntity>>("currency");
            if(list==null)
            {
                list = DataService.GetList<currencyEntity>();
                _cache.Set("currency", list, 600, false);
            }
            return list;
        }
        public currencyEntity GetCurrencyByMarket(string market="cn")
        {
            var list = GetCurrencyList();
            return list.Where(o => o.code.Contains(market,StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }
        public currencyEntity GetCurrencyById(int id)
        {
            var list = GetCurrencyList();
            return list.Where(o => o.id == id).FirstOrDefault();
        }
        public currencyEntity GetCurrencyByCode(string code)
        {
            var list = GetCurrencyList();
            return list.Where(o => o.code.Contains(code,StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }       
        public int InsertMarketBuyRecord(marketbuyrecordEntity record)
        {
            var count= DataService.Insert<marketbuyrecordEntity>(record);
            return count;
        }
        public int InsertMarketSellRecord(marketsellrecordEntity record)
        {
            var count = DataService.Insert<marketsellrecordEntity>(record);
            return count;
        }
        public marketbuyrecordEntity GetMarketRecordByOrderId(string orderid)
        {
            var record = DataService.Get<marketbuyrecordEntity>(o => o.orderid == orderid);
            return record;
        }
        public marketsellrecordEntity GetMarketSellRecordByOrderId(string orderid)
        {
            var record = DataService.Get<marketsellrecordEntity>(o => o.orderid == orderid);
            return record;
        }
        public changerEntity GetChangerByCid(int cid)
        {
            var changer = DataService.Get<changerEntity>(o => o.cid == cid);
            return changer;
        }      
        public marketbuyrecordEntity GetMarketRecordByFlag(int uid, int flag)
        {
            var record = DataService.Get<marketbuyrecordEntity, int>(o => o.flag == flag && o.uid==uid, o => o.id, Dapper.Extension.Core.EOrderBy.Desc);
            return record;
        }
        /// <summary>
        /// 获取未处理订单
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public marketbuyrecordEntity GetPendingMarketRecordByUid(int uid)
        {
            var record = DataService.Get<marketbuyrecordEntity, int>(o => o.flag>=21 && o.uid == uid, o => o.id, Dapper.Extension.Core.EOrderBy.Desc);
            return record;
        }
        public int UpMarketRecordFlag(string orderid,int uid,int flag)
        {
            int count = 0;
            DataService.Conn(conn =>
            {
                count=conn.CommandSet<marketbuyrecordEntity>().Where(o => o.uid == uid && o.orderid==orderid).Update(o => new marketbuyrecordEntity
                {
                    flag=flag
                });
            });
            return count;
        }
        public int UpMarketRecordUPayTime(string orderid,int uid,long payTime)
        {
            int count = 0;
            DataService.Conn(conn =>
            {
                count = conn.CommandSet<marketbuyrecordEntity>().Where(o => o.uid == uid && o.orderid==orderid).Update(o => new marketbuyrecordEntity
                {
                    upaytime=payTime,
                    flag=22
                });
            });
            return count;
        }
        #region 场外出售
        public marketsellrecordEntity GetPendingSellRecord(int uid)
        {
            var record = DataService.Get<marketsellrecordEntity, int>(o => o.flag >= 41 && o.uid == uid, o => o.id, Dapper.Extension.Core.EOrderBy.Desc);
            return record;
        }
        /// <summary>
        /// 用户确认收款
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int MarketSellUConfirm(string orderid,int flag)
        {
            int count = 0;
            DataService.Conn(conn =>
            {
                count = conn.CommandSet<marketsellrecordEntity>().Where(o=>o.orderid == orderid).Update(o => new marketsellrecordEntity
                {
                    flag=1
                });
            });
            return count;
        }
        public int UpMarketSellFlag(string orderid,int flag)
        {
            int count = 0;
            DataService.Conn(conn =>
            {
                count = conn.CommandSet<marketsellrecordEntity>().Where(o => o.orderid == orderid).Update(o => new marketsellrecordEntity
                {
                    flag = flag
                });
            });
            return count;
        }
        #endregion
    }
}
