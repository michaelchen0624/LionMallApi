using DataCommon;
using Lion.Entity;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lion.Services
{
    public class AssetService
    {
        private ICacheService _cache;
        public AssetService(ICacheService cache)
        {
            _cache = cache;
        }
        public IEnumerable<currencyEntity> GetCurrencyList()
        {
            var list = _cache.Get<IEnumerable<currencyEntity>>("currencyList");
            if (list == null)
            {
                list = DataService.GetList<currencyEntity>();
                _cache.Set("currencyList", list, 600, false);
            }
            return list;
        }
        public IEnumerable<integraltypeEntity> GetIntegralList()
        {
            var list = _cache.Get<IEnumerable<integraltypeEntity>>("integralList");
            if(list==null)
            {
                list = DataService.GetList<integraltypeEntity>();
                _cache.Set("integralList", list, 300, false);
            }
            return list;
        }
        public IEnumerable<integraltypeEntity> GetIntegralListByCoinType(int cointype=2)
        {
            return DataService.GetList<integraltypeEntity>(o => o.coin_type == cointype);
        }

        public integraltypeEntity GetIntegralMain()
        {
            var list = GetIntegralList();
            return list.Where(o => o.main_flag == 1).FirstOrDefault();
        }
        public currencyEntity GetCurrencyByMarket(string market = "cn")
        {
            var list = GetCurrencyList();
            return list.Where(o => o.code.Contains(market, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }
        public currencyEntity GetCurrencyById(int id)
        {
            var list = GetCurrencyList();
            return list.Where(o => o.id == id).FirstOrDefault();
        }
        public currencyEntity GetCurrencyByCode(string code)
        {
            var list = GetCurrencyList();
            return list.Where(o => o.code.Contains(code, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }
        public exchangerateEntity GetRateByCurrencyCode(string currencyCode)
        {
            return DataService.Get<exchangerateEntity>(o => o.currency_code == currencyCode);
        }
        public exchangerateEntity GetRateByCurrencyId(int id)
        {
            return DataService.Get<exchangerateEntity>(o => o.currency_id == id);
        }
        public integraltypeEntity GetAssetById(int id)
        {
            return DataService.Get<integraltypeEntity>(o => o.id == id);
        }
        public integraltypeEntity GetAssetByCName(string cname)
        {
            return DataService.Get<integraltypeEntity>(o => o.c_name == cname);
        }
        public rechargeaddrEntity GetRechargeAddrByAddress(string address)
        {
            address = address.ToLower();
            return DataService.Get<rechargeaddrEntity>(o => o.addr == address);
        }
        /// <summary>
        /// 获取充值地址
        /// </summary>
        /// <param name="usn"></param>
        /// <param name="assetcode"></param>
        /// <returns></returns>
        public rechargeaddrEntity GetRechargeAddrByUsnAndCoinType (string usn,int cointype)
        {
            return DataService.Get<rechargeaddrEntity>(o => o.usn == usn && o.coin_type == cointype);
        }
        /// <summary>
        /// 创建充值地址
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertRechargeAddr(rechargeaddrEntity entity)
        {
            return DataService.Insert<rechargeaddrEntity>(entity,o=>o.coin_type==entity.coin_type && o.usn==entity.usn);
        }
        public int UpdateRechargeAddr(int uid,string addr)
        {
            return DataService.Update<rechargeaddrEntity>(o => o.uid == uid, o => new rechargeaddrEntity
            {
                addr=addr
            });
        }
        /// <summary>
        /// 获取充值地址
        /// </summary>
        /// <param name="usn"></param>
        /// <param name="assetcode"></param>
        /// <returns></returns>
        public integralAddr GetIntegralAddr(string usn,int assetcode)
        {
            return new integralAddr
            {
                addr= "18PvP5AAUuQUubYd1EXravq3hxJ7ShrMBw",
                key= "L5cNA52BuPQae88A11T8Jknd1rQqerzaFZ8mqLArygiai6DRLz91"
            };
        }
        public configEntity GetConfigByKey(string config_key)
        {
            return DataService.Get<configEntity>(o => o.config_key == config_key);
        }
        /// <summary>
        /// 获取发行价 0.1 usdt/gcoin
        /// </summary>
        /// <returns></returns>
        public double GetIssuePrice()
        {
            return 0.1;
        }
    }
    public class integralAddr
    {
        public string addr { get; set; }
        public string key { get; set; }
    }
}
