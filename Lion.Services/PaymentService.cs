using DataCommon;
using Lion.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Extension.MsSql;

namespace Lion.Services
{
    public class PaymentService
    {
        public int InsertPayMent(upaymentEntity data)
        {
            return DataService.Insert<upaymentEntity>(data);
        }
        /// <summary>
        /// 获取兑换商收款方式
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public IEnumerable<cpaymentEntity> GetCPayMentListByCid(int cid)
        {
            var list = DataService.GetList<cpaymentEntity>(o => o.cid == cid);
            return list;
        }
        /// <summary>
        /// 获取用户可用收款方式数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int GetUPayMentCount(int uid)
        {
            var count = DataService.GetCount<upaymentEntity>(o => o.uid == uid && o.active == 1 && o.del != 1);
            return count;
        }
        /// <summary>
        /// 获取用户收款方式
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IEnumerable<upaymentEntity> GetUPayMentListByUid(int uid)
        {
            var list = DataService.GetList<upaymentEntity>(o => o.uid == uid && o.del != 1);
            return list;
        }
        /// <summary>
        /// 获取可用用户收款方式
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IEnumerable<upaymentEntity> GetActiveUPayMentListByUid(int uid)
        {
            var list = DataService.GetList<upaymentEntity>(o => o.uid == uid && o.active==1 && o.del != 1);
            return list;
        }
        /// <summary>
        /// 获取收款方式列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<paymentEntity> GetPayMentList()
        {
            var list = DataService.GetList<paymentEntity>();
            return list;
        }
        public IEnumerable<paymentfieldEntity> GetPayMentFieldByPid(int pid, string lang = "cn")
        {
            var list = DataService.GetList<paymentfieldEntity>(o => o.pid == pid && o.lang == lang);
            return list;
        }
        public upaymentEntity GetUPaymentByGuid(string guid)
        {
            return DataService.Get<upaymentEntity>(o => o.guid == guid);
        }
        public int SetUPayMentActive(int uid,int pid, int active)
        {
            //int count = 0;
            //var value = active == 1 ? 1 : 0;
            //DataService.Conn(conn =>
            //{
            //    count= conn.CommandSet<upaymentEntity>().Where(o=>o.uid==uid && o.guid==guid).Update(o=>new upaymentEntity
            //    {
            //        active=value
            //    });
            //});
            //return count;
            int count= DataService.Update<userEntity>(o => o.uid == uid, o => new userEntity
            {
                d_payment=pid
            });
            return count;
        }
        public int DelUPayMent(int uid,string guid)
        {
            int count = 0;
            DataService.Conn(conn =>
            {
                count = conn.CommandSet<upaymentEntity>().Where(o => o.uid == uid && o.guid==guid).Update(o=>new upaymentEntity
                {
                    del=1
                });
            });
            return count;
        }
    }
}
