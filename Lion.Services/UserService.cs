using DataCommon;
using Lion.Entity;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Lion.ViewModel.respModel;
//using Dapper.Extension.MsSql
using LionMall.Tools;
using Lion.ViewModel.requestModel;

namespace Lion.Services
{
    public class UserService
    {
        private ICacheService _cache;
        private AssetService _assetService;
        private ILogService _log;
        private RpcNotifyService _rpcService;
        public UserService(ICacheService cache,AssetService assetService,ILogService log,RpcNotifyService rpcService)
        {
            _cache = cache;
            _assetService = assetService;
            _log = log;
            _rpcService = rpcService;
        }
        public int SetUserInfo(string username,string iconurl,string usn)
        {

            int count= DataService.Update<userEntity>(o => o.guid == usn, o => new userEntity
            {
                username = string.IsNullOrWhiteSpace(username) ? "" : username,
                iconurl = string.IsNullOrWhiteSpace(iconurl) ? "" : iconurl
            });
            if (count > 0)
                _cache.Remove("/User/" + usn);
            return count;
        }
        public userEntity GetUserByRecommend(string recommend)
        {
            recommend = recommend.ToUpper();
            return DataService.Get<userEntity>(o => o.recommend == recommend);
        }
        public userEntity GetUserByUsnAndPwd(string usn, string pwd)
        {
            return DataService.Get<userEntity>(o => o.guid == usn && o.passwd == pwd);
        }
        public userEntity GetUserByUsnAndPaypwd(string usn,string pwd)
        {
            return DataService.Get<userEntity>(o => o.guid == usn && o.paypwd == pwd);
        }
        public userEntity GetUserByUsnNotCache(string guid)
        {
            return DataService.Get<userEntity>(o => o.guid == guid);
        }
        public userEntity GetUserByUserSN(string guid)
        {
            var user = _cache.Get<userEntity>("/User/" + guid);
            if(user==null)
            {
                user = DataService.Get<userEntity>(o => o.guid == guid);
                if (user != null)
                    _cache.Set("/User/" + guid, user, 300, false);
            }
            return user;
        }
        /// <summary>
        /// 根据uid获取层级表
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IEnumerable<levelextendEntity> GetExtendListByUid(int uid)
        {
            return DataService.GetList<levelextendEntity>(o => o.uid == uid);
        }
        public userbalanceEntity GetBalanceByUserSN(string guid)
        {
            return DataService.Get<userbalanceEntity>(o => o.usn == guid);
        }
        public int InsertUser(userEntity user)
        {
            var count= DataService.Insert<userEntity>(user);
            return count;
        }
        public userEntity GetUserByPhoneAndPwd(string phone,string pwd,string phonearea="86")
        {
            if (string.IsNullOrWhiteSpace(phonearea))
                phonearea = "86";
            return DataService.Get<userEntity>(o => o.phone == phone && o.phonearea == phonearea && o.passwd == pwd);
        }
        public int ChangerUserPwdBySn(string sn, string newpwd)
        {
            return DataService.Update<userEntity>(o => o.guid == sn,o=> new userEntity { passwd = newpwd });
        }
        public int ChangerUserPayPwdBySn(string sn, string newpwd)
        {
            int count= DataService.Update<userEntity>(o => o.guid == sn, o => new userEntity { paypwd = newpwd });
            if (count > 0)
                _cache.Remove(CacheKey.UserSn + sn);
            return count;
        }
        public userEntity GetUserByPhone(string phone,string phonearea="86")
        {
            if (string.IsNullOrWhiteSpace(phonearea))
                phonearea = "86";
            return DataService.Get<userEntity>(o => o.phone == phone && o.phonearea == phonearea);
        }
        public string GetPwdMD5(string pwd)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var result= md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwd));
            var str = BitConverter.ToString(result);
            return str.Replace("-","");
        }
        public string GetRecommendNum()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            var num= r.Next(1000, 16777216);
            var ret= num.ToString("X");
            if(ret.Length<6)
            {
                var patchstr = new StringBuilder();
                for(int i=0;i<6-ret.Length;i++)
                {
                    patchstr.Append("0");
                }
                ret = $"{patchstr.ToString()}{ret}";
            }
            return ret;
            //var num = LionRandom.GenerateRandomCode(8);
            //while (true)
            //{
            //    int number = TypeHelper.StringToInt(num);
            //    if (number < 10000 || number > 16777216)
            //    {
            //        num = LionRandom.GenerateRandomCode(8);
            //        continue;
            //    }
            //    if (GetUserByRecommend(num.ToString("X"))!=null)
            //        num = LionRandom.GenerateRandomCode(6);
            //    else
            //        break;
            //}
            //return num;
        }


        #region 计算层级
        public userextendEntity GetUserExtendByUsn(string usn)
        {
            return DataService.Get<userextendEntity>(o => o.usn == usn);
        }
        public userextendEntity GetUserExtendByUid(int uid)
        {
            return DataService.Get<userextendEntity>(o => o.uid == uid);
        }
        /// <summary>
        /// 计算级别 有修改的话会修改,检查，自己和线的级别
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int LevelCalculate(int uid,bool calParent=true)
        {
            var userextend = GetUserExtendByUid(uid);//获取用户扩展表
            var level = GetLevelByAmount(userextend.total_consum);//消费金额对应的level
            int cal = 0;
            if (level == userextend.level)
                return 1;
            //if(level!=userextend.level)//加一个判断是不是充值导致升级
            //{
            //    cal=HandleLevelByUid(uid,userextend.total_consum,userextend.level);
            //}
            cal = HandleLevelByUid(uid, userextend.total_consum, userextend.level);
            if (calParent)//计算推荐层级标志
            {
                foreach (var item in GetParentLevelInfo(uid).OrderByDescending(o => o.uid))
                {
                    HandleLevelByUid(item.uid, item.total_consum, item.level);
                }
            }            
            return cal;
        }
        /// <summary>
        /// 处理用户层级
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="consum">累计消费</param>
        /// <param name="level">当前层级</param>
        /// <returns></returns>
        public int HandleLevelByUid(int uid,double consum,int level)
        {
            //var list = GetLevelConfigList();//获取级别配置表
            //var userextend = GetUserExtendByUid(uid);
            //var level = GetLevelByUser(uid, userextend.total_consum);
            var userlevel = GetLevelByUser(uid, consum);//获取用户级别
            //if (level == userextend.level)
            if(level==userlevel)
                return 1;
            try
            {
                DataService.Transcation(tc =>
                {
                    int count1 = tc.CommandSet<userextendEntity>().Where(o => o.uid == uid).
                    Update(o => new userextendEntity
                    {
                        level = userlevel
                    });
                    int count2 = tc.CommandSet<upgradeEntity>().Insert(new upgradeEntity
                    {
                        uid = uid,
                        origin_level = level,
                        level = userlevel,
                        times = DateTime.Now.GetTimeUnixLocal()
                    });
                    if (count1 <= 0 || count2 <= 0)
                    {
                        _log.E("userLevel", $"{uid},层级更新错误,当前level:{level},计算level:{userlevel}");
                        throw new PayException("保存级别失败", 404);
                    }

                });
            }
            catch(Exception ex)
            {
                return 0;
            }
            return 1;
            //var flag = GetLevelAccordByUid(uid, level);
            //if (!flag)//条件不成立
            //{
            //    while (true)
            //    {
            //        level = level - 1;
            //        if (level == 1)
            //            break;
            //        bool accordFlag = GetLevelAccordByUid(uid, level);
            //        if (accordFlag)
            //            break;
            //    }
            //}
            //int count1 = DataService.Update<userextendEntity>(o => o.uid == uid, o => new userextendEntity
            //{
            //    level = level
            //});
            //if (count1 <= 0)
            //{
            //    //不成功处理
            //}
            //HandleParentLevel(uid);
            //return 0;
        }       
        /// <summary>
        /// 根据消费金额获得对应的级别
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public int GetLevelByAmount(double amount)
        {
            var list = GetLevelConfigList();
            int level = 6;
            foreach(var item in list.OrderByDescending(o=>o.total_amount))
            {
                if (amount >= item.total_amount)
                    break;
                level = level - 1;
            }
            return level <= 0 ? 1 : level;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int GetLevelByCount(IEnumerable<levelCount> list)
        {
            int level = 0;
            levelCount levelitem=null ;
            int count = 0;
            foreach (var item in list.OrderByDescending(o=>o.level))
            {
                //if (item.count > 0 && item.count < 3)
                //    count += item.count;
                if((item.count+count)>=3)
                {
                    levelitem = item;
                    break;
                }
                else
                {
                    count += item.count;
                }
                //if (item.count >= 3)
                //{
                //    levelitem = item;
                //    break;
                //}
                
            }
            if (levelitem == null)
                levelitem = new levelCount { level = 0, count = 0 };
            foreach(var item in GetLevelConfigList().OrderByDescending(o=>o.id))
            {
                if(item.direct_count<=levelitem.count && item.direct_level==levelitem.level)
                {
                    level = item.id;
                    break;
                }
            }
            return level <= 2 ? 2 : level;
        }
        public IEnumerable<userlevelconfigEntity> GetLevelConfigList()
        {
            var list = _cache.Get<IEnumerable<userlevelconfigEntity>>("level_list");
            if(list==null || list.Count()<=0)
            {
                list= DataService.GetList<userlevelconfigEntity>(o => o.id > 0);
                _cache.Set("level_list", list, 600, false);
            }
            return list;
        }
        /// <summary>
        /// 获得用户的级别
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="amount">累计消费</param>
        /// <returns></returns>
        public int GetLevelByUser(int uid, double amount)
        {
            var amountLevel = GetLevelByAmount(amount);
            var countLevel = GetLevelByCount(GetSubLevelCount(uid));
            return amountLevel >= countLevel ? countLevel : amountLevel;
        }
        ///// <summary>
        ///// 是不是符合级别条件
        ///// </summary>
        ///// <param name="uid"></param>
        ///// <param name="level"></param>
        ///// <returns></returns>
        //public bool GetLevelAccordByUid(int uid,int testlevel)
        //{
        //    if (testlevel == 1)
        //        return true;
        //    var level = GetLevelConfigList().Where(o => o.id == testlevel).FirstOrDefault();
        //    if(level.direct_level==0)
        //    {
        //        var userextend = GetUserExtendByUid(uid);
        //        return userextend.total_consum >= level.total_amount;
        //    }
        //    int count= DataService.GetCount<userextendEntity>(o => o.parent == uid && o.level == level.direct_level);
        //    return count >= level.direct_count;
        //}
        public IEnumerable<levelInfo> GetParentLevelInfo(int uid)
        {
            var sql = "select b.uid,b.usn,b.level,b.total_consum from lt_levelextend as a,lt_userextend as b where a.parent=b.uid and a.uid=@uid";
            var list = DataService.QueryListBySql<levelInfo>(sql, new { uid = uid });
            return list;
        }
        /// <summary>
        /// 获取子类的级别数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IEnumerable<levelCount> GetSubLevelCount(int uid)
        {
            var sql = "select [level] ,count(1) as count from lt_userextend where parent=@uid group by level ";
            var list = DataService.QueryListBySql<levelCount>(sql, new { uid = uid });
            return list;
        }
        /// <summary>
        /// 获取二级子类的级别数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IEnumerable<levelCount> GetSencondLevelCount(int uid)
        {
            var sql = "select b.level as level ,count(1) as count from lt_levelextend as a,lt_userextend as b where a.uid=b.uid and a.parent=@parent and a.level=2 group by b.level ";
            var list = DataService.QueryListBySql<levelCount>(sql, new { parent = uid });
            return list;
        }
        #endregion
        /// <summary>
        /// 获取直接推荐人数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int GetDirectCount(int uid)
        {
            return DataService.GetCount<userlevelEntity>(o => o.parent == uid);
        }
        public int GetInDirectCount(int uid)
        {
            return DataService.GetCount<levelextendEntity>(o => o.parent == uid && o.level == 2);
        }
        public inviteuser InviteUser(int uid)
        {
            var sql = "";
            return DataService.QueryEntityBySql<inviteuser>(sql, new { uid = uid });
        }
        public IEnumerable<userextend> GetUserExtendListByParent(int parent,int pageIndex,int pageCount)
        {
            var sql = new StringBuilder();
            sql.AppendLine("SELECT COUNT(1) FROM [lt_userextend] WHERE ([parent] = @parent) ; ");
            sql.AppendLine($"SELECT  TOP {pageCount}  [uid] AS [uid],[usn] AS [usn],[parent] AS [parent],[username] AS [username],[level] AS [level],[total_consum] AS [total_consum],[iconurl] AS [iconurl] ,[phone] as [phone],[phonearea] as [phonearea]");
            sql.AppendLine(" FROM    ( SELECT a.uid,a.usn,a.parent,a.level,a.total_consum,b.[username] as [username],b.[iconurl] as [iconurl], b.[phone] as [phone],b.[phonearea] as [phonearea],ROW_NUMBER() OVER ( ORDER BY a.[uid] ASC  ) AS ROWNUMBER FROM  [lt_userextend] as a left join [lt_user] as b  ");
            sql.AppendLine("on a.uid=b.uid  WHERE  ([parent] = @parent)  ) T ");
            sql.AppendLine($"WHERE   ROWNUMBER > {(pageIndex-1)*pageCount} AND ROWNUMBER <= {pageIndex*pageCount} ORDER BY [uid] ASC ;");
            var list = DataService.QueryPageListBySql<userextend>(sql.ToString(),new { parent=parent},pageIndex, pageCount);
            return list;
        }
        /// <summary>
        /// 获取间接推荐
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public IEnumerable<userextend> GetUserExtendTwoListByParent(int parent, int pageIndex, int pageCount)
        {
            var sql = new StringBuilder();
            sql.AppendLine("SELECT COUNT(1) FROM [lt_levelextend] WHERE ([parent] = @parent and level=2) ; ");
            sql.AppendLine($"SELECT  TOP {pageCount}  [uid] AS [uid],[usn] AS [usn],[parent] AS [parent],[username] AS [username],[level] AS [level],[total_consum] AS [total_consum],[iconurl] AS [iconurl] ,[phone] as [phone],[phonearea] as [phonearea]");
            sql.AppendLine(" FROM    ( SELECT a.uid,b.usn,b.parent,b.level,b.total_consum,c.[username] as [username],c.[iconurl] as [iconurl], c.[phone] as [phone],c.[phonearea] as [phonearea],ROW_NUMBER() OVER ( ORDER BY a.[uid] ASC  ) AS ROWNUMBER FROM  [lt_levelextend] as a left join [lt_userextend] as b  ");
            sql.AppendLine("on a.uid=b.uid  left join lt_user as c on c.uid=a.uid  WHERE  (a.[parent] = @parent and a.level=2)  ) T ");
            sql.AppendLine($"WHERE   ROWNUMBER > {(pageIndex - 1) * pageCount} AND ROWNUMBER <= {pageIndex * pageCount} ORDER BY [uid] ASC ;");
            var list = DataService.QueryPageListBySql<userextend>(sql.ToString(), new { parent = parent }, pageIndex, pageCount);
            return list;
        }

        /// <summary>
        /// 获得推荐子类的数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public subusercount GetSubCountByUid(int uid)
        {
            var sql = "select count(1) as direct_count ,(select count(1) from lt_levelextend where parent=@uid and level=2) as indirect_count from lt_userlevel where parent=@uid";
            return DataService.QueryEntityBySql<subusercount>(sql, new { uid = uid });
        }
        public (subusercount,user) GetInviteView(int uid)
        {
            var sql = new StringBuilder();
            sql.Append("select count(1) as direct_count ,(select count(1) from lt_levelextend where parent=@uid and level=2) as indirect_count from lt_userlevel where parent=@uid ;");
            sql.Append(" select b.uid,b.guid,b.phone,b.phonearea,b.username,b.fullname,b.idnum,b.recommend,b.times ");
            sql.Append("from lt_userlevel as a left join lt_user as b on a.parent=b.uid  where a.uid=@uid ");
            return DataService.GetMultiple<subusercount, user>(sql.ToString(), new { uid = uid });
        }
        public int SetFlash(int uid,int rate)
        {
            int count= DataService.Update<walletEntity>(o => o.uid == uid, o => new walletEntity
            {
                flash_rate=rate
            });
            return count;
        }
        public walletEntity GetWalletByUid(int uid)
        {
            return DataService.Get<walletEntity>(o => o.uid == uid);
        }
        #region 获取数据中心
        /// <summary>
        /// 获取数据中心
        /// </summary>
        /// <returns></returns>
        public DCViewModel GetDataCenterView()
        {            
            var model = new DCViewModel();
            BindDataUser(model);//绑定用户
            BindDataConsumAndBuy(model);//绑定消费数据
            BindDataKline(model);//绑定k线数据
            //BindDataBuy(model);//绑定回购数据
            return model;
        }
        /// <summary>
        /// 绑定用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private void BindDataUser(DCViewModel model)
        {
            var total_count = DataService.GetCount<userEntity>(o => o.uid>0);
            var now= DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day).GetTimeUnixLocal();
            model.total_reg = total_count;
            model.current_reg = DataService.GetCount<userEntity>(o=>o.times>=startTime);
            //return model;
        }
        /// <summary>
        /// 绑定消费
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private void BindDataConsumAndBuy(DCViewModel model)
        {
            var asset = _assetService.GetIntegralMain();
            model.unit = asset.c_unit;
            var now = DateTime.Now;
            var batch = now.AddDays(-1).ToString("yyyy-MM-dd");
            var previous = now.AddDays(-2).ToString("yyyy-MM-dd");
            var settle = DataService.Get<settlerecordEntity>(o => o.settle_date == batch);
            if (settle == null)
                settle = DataService.Get<settlerecordEntity>(o => o.settle_date == previous);
            if (settle==null)
            {
                model.last_avg = "0.000";
                model.last_flash = "0.000";
                model.last_consum = "0.000";
                model.last_profit = "0.000";
                model.last_issue = "0.000";
            }
            else
            {
                var profit = Math.Round(settle.total_recharge * 0.28,4);
                model.last_avg = settle.total_avg.ToString("N3");
                model.last_flash = settle.total_flash.ToString("N3");
                model.last_consum = settle.total_recharge.ToString("N3");
                model.last_profit = profit.ToString("N3");
                model.last_issue = settle.total_issue.ToString("N3");
            }
            var platdata = DataService.Get<platdataEntity>(o => o.pid == 1);
            var t_profit = Math.Round(platdata.total_recharge * 0.28, 4);
            var total_buy = platdata.flash_us + platdata.avg_us;
            var total_balance = t_profit - total_buy;
            model.total_consum = platdata.total_recharge.ToString("N3");
            model.total_profit = t_profit.ToString("N3");
            model.total_issue = platdata.total_issue.ToString("N3");
            model.total_flash = platdata.total_flash.ToString("N3");
            model.total_avg = platdata.total_avg.ToString("N3");
            model.total_buy = total_buy.ToString("N3");
            model.balance = total_balance.ToString("N3");
        }
        /// <summary>
        /// 绑定k线
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private void BindDataKline(DCViewModel model)
        {
            var p_list = GetPriceRecordList();
            var count = p_list.Count();
            var buy_date = new string[count];
            var buy_price = new double[count];
            var issue_date = new string[count];
            var issue_price = new double[count];
            int i = 0;
            foreach (var item in p_list)
            {
                buy_date[i] = item.batch;
                issue_date[i] = item.batch;
                buy_price[i] = item.buy_price;
                issue_price[i] = item.issue_price;
                i++;
            }
            model.buy_date = buy_date;
            model.buy_price = buy_price;
            model.issue_date = issue_date;
            model.issue_price = issue_price;
            //return model;
        }
        ///// <summary>
        ///// 绑定回购
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //private void BindDataBuy(DCViewModel model)
        //{
        //    //return model;
        //}
        #endregion
        public IEnumerable<pricerecordEntity> GetPriceRecordList()
        {
            return DataService.GetList<pricerecordEntity>();
        }
        /// <summary>
        /// 获取用户订单编号
        /// </summary>
        /// <returns></returns>
        public string GetUserOrderNumber()
        {
            return $"{DateTime.Now.ToString("yyyyMMdd")}{StringHelper.GuidToLongID()}";
        }

        public int SetRecommend(userEntity user, userEntity recommend=null)
        {
            if (recommend == null)
            {
                int count= DataService.Update<userEntity>(o => o.uid == user.uid, o => new userEntity
                {
                    origin = 0
                });
                return count;
            }
            var list = GetExtendListByUid(recommend.uid);
            var times = DateTime.Now.GetTimeUnixLocal();
            int count1 = 0;
            DataService.Transcation(tc =>
            {
                int count2=tc.CommandSet<userEntity>().Where(o=>o.uid==user.uid).Update(o => new userEntity
                {
                    origin=recommend.uid
                });
                int count3 = tc.CommandSet<userlevelEntity>().Insert(new userlevelEntity
                {
                    uid = user.uid,
                    parent = recommend.uid,
                    times = times
                });
                int count4 = tc.CommandSet<levelextendEntity>().Insert(new levelextendEntity
                {
                    level = 1,
                    parent = recommend.uid,
                    uid = user.uid,
                    times = times
                });
                int count5 = tc.CommandSet<userextendEntity>().Where(o=>o.uid==user.uid).Update(o=>new userextendEntity
                {
                    parent = recommend.uid
                });
                foreach (var item in list)
                {
                    count1= tc.CommandSet<levelextendEntity>().Insert(new levelextendEntity
                    {
                        level = item.level + 1,
                        parent = item.parent,
                        uid = user.uid,
                        times = times
                    });
                    if (count1 <= 0)
                        break;
                }
                if (list.Count() == 0)
                    count1 = 1;
                if (count1 <= 0 || count2 <= 0 || count3 <= 0 || count4 <= 0 || count5 <= 0 )
                    throw new LionException("设置不成功", 1401);
            });
            return 1;
        }
        public string GetRechargeAddrByUsnAndCoinType(string usn, int cointype)
        {
            var addr = _assetService.GetRechargeAddrByUsnAndCoinType(usn, cointype);
            string address = string.Empty;
            if (addr == null)
            {
                //_log.I("test", "取不到address的值");
                //address = _rpcService.GetNewAddr(new ViewModel.requestModel.reqChainData
                //{
                //    plat = 2,
                //    usn = usn
                //});
                address = _rpcService.GetAddr(new reqAddress { pwd = usn });
            }
            else
            {
                return addr.addr;
            }

            if (!string.IsNullOrWhiteSpace(address))
            {
                var user = GetUserByUserSN(usn);
                int count=_assetService.InsertRechargeAddr(new rechargeaddrEntity
                {
                    uid=user.uid,
                    usn=usn,
                    coin_type=cointype,
                    addr=address
                });
                if (count <= 0)
                    _log.E("SaveAddr", $"uid:{user.uid},usn:{usn},address:{address}保存到数据库失败");
            }
            else
            {
                _log.E("SaveAddr", $"usn:{usn},cointype:{cointype}生成地址失败");
            }
            return string.IsNullOrWhiteSpace(address) ? "" : address;
        }

        public respNewUser GetNewUserTotal(DateTime fromDate ,DateTime toDate)
        {
            var from = fromDate.GetTimeUnixLocal();
            var to = toDate.GetTimeUnixLocal();
            var totalcount = DataService.GetCount<userEntity>(o => o.times >= from && o.times <= to);
            var sql = new StringBuilder();
            sql.Append("select a.phonearea as phonearea,a.phone as phone,a.username as username,b.level as level,b.total_consum as total_consum,a.times as times ");
            sql.Append("from lt_user as a left join lt_userextend as b on a.uid=b.uid ");
            sql.Append("where a.times>=@from and a.times<=@to");
            var u_list=DataService.QueryListBySql<newuser>(sql.ToString(), new { from = from, to = to });
            var list = new List<respNewUserItem>();
            foreach (var item in u_list)
            { 
                list.Add(new respNewUserItem
                {
                    username=item.username,
                    phone=item.phone,
                    phonearea=item.phonearea,
                    totalConsum=item.total_consum.ToString("N3"),
                    regTime=DateTime.Now.GetDateTimeLocal(item.times).ToString("yyyy-MM-dd HH:mm:ss"),
                    level=GetLevelNameByLevel(item.level)
                });
            }
            return new respNewUser
            {
                from = fromDate.ToString("yyyy-MM-dd HH:mm:ss"),
                to = toDate.ToString("yyyy-MM-dd HH:mm:ss"),
                list = list,
                totalCount = totalcount
            };
        }
        private string GetLevelNameByLevel(int level)
        {
            var levelName = string.Empty;
            switch (level)
            {
                case 1:
                    levelName = "大众会员";
                    break;
                case 2:
                    levelName = "青铜会员";
                    break;
                case 3:
                    levelName = "白银会员";
                    break;
                case 4:
                    levelName = "黄金会员";
                    break;
                case 5:
                    levelName = "铂金会员";
                    break;
                case 6:
                    levelName = "钻石会员";
                    break;
                default:
                    break;
            }
            return levelName;
        }

        public respNewIssue GetNewIssueTotal(string from ,string to)
        {
            var sql = new StringBuilder();
            sql.Append("select * from lt_settlerecord where settle_date>=@from and settle_date<=@to");
            var d_list = DataService.QueryListBySql<settlerecordEntity>(sql.ToString(), new { from = from, to = to });
            var list = new List<NewIssue>();
            foreach (var item in d_list)
            {
                list.Add(new NewIssue
                {
                    date=item.settle_date,
                    recharge=item.total_recharge.ToString("N3"),
                    flash=item.total_flash.ToString("N3"),
                    flash_us=item.flash_us.ToString("N3"),
                    issue=item.total_issue.ToString("N3")
                });
            }
            return new respNewIssue
            {
                from = from,
                to = to,
                list = list
            };
        }
        public int SetGift(userEntity user ,double amount)
        {
            if (user == null)
                throw new LionException("找不到用户", 1401);
            int uid = user.uid;
            DataService.Transcation(tc =>
            {
                int count= tc.CommandSet<walletEntity>().Where(o => o.uid == uid).Incre(o => new walletEntity
                {
                    balance = amount
                }).UpdateOnlyIncre();
                int count1= tc.CommandSet<giftEntity>().Insert(new giftEntity
                {
                    uid=uid,
                    usn=user.guid,
                    phonearea=user.phonearea,
                    phone=user.phone,
                    amount=amount,
                    times=DateTime.Now.GetTimeUnixLocal()
                });
                if (count1 <= 0 || count1 <= 0 )
                    throw new LionException("设置不成功", 1401);
            });
            return 1;
        }
        public double GetTotalUserGift(int uid)
        {
            var sql = new StringBuilder();
            sql.Append("select sum(amount) as amount from lt_gift where uid=@uid");
            var giftamount= DataService.QueryEntityBySql<giftAmount>(sql.ToString(), new { uid = uid });
            return giftamount.amount;
        }
        public userGift GetUserGift(DateTime fromDate, DateTime toDate)
        {
            var from = fromDate.GetTimeUnixLocal();
            var to = toDate.GetTimeUnixLocal();
            var totalcount = DataService.GetCount<giftEntity>(o => o.times >= from && o.times <= to);
            var g_list = DataService.GetList<giftEntity>(o => o.times >= from && o.times <= to);
            //var sql = new StringBuilder();
            //sql.Append("select a.phonearea as phonearea,a.phone as phone,a.username as username,b.level as level,b.total_consum as total_consum,a.times as times ");
            //sql.Append("from lt_user as a left join lt_userextend as b on a.uid=b.uid ");
            //sql.Append("where a.times>=@from and a.times<=@to");
            //var u_list = DataService.QueryListBySql<newuser>(sql.ToString(), new { from = from, to = to });
            var list = new List<userGiftItem>();
            foreach (var item in g_list)
            {
                list.Add(new userGiftItem
                {
                    amount = item.amount.ToString("N2"),
                    phone = item.phone,
                    phonearea = item.phonearea,
                    times = DateTime.Now.GetDateTimeLocal(item.times).ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }
            return new userGift
            {
                from = fromDate.ToString("yyyy-MM-dd HH:mm:ss"),
                to = toDate.ToString("yyyy-MM-dd HH:mm:ss"),
                list = list,
                totalCount = totalcount
            };
        }
    }
}
