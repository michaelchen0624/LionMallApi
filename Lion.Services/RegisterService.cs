using DataCommon;
using Lion.Entity;
using Prod.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lion.Services
{
    public class RegisterService
    {
        private UserService _userService;
        public RegisterService(UserService userService)
        {
            _userService = userService;
        }
        public string RegUser(string username,string phonearea,string phone,string passwd,userEntity recommend=null)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var pwd= SecureHelper.AESEncrypt(passwd, "lion");
            var times = DateTime.Now.GetTimeUnixLocal();
            var recommendnum = _userService.GetRecommendNum();
            //phonearea = StringHelper.GetNumberFromStr(phonearea);
            var user = new userEntity
            {
                username = username,
                guid = guid,
                passwd = pwd,
                phone = phone,
                phonearea = phonearea,
                times = times,
                recommend = recommendnum
            };
            var userbalance = new userbalanceEntity
            {
                //asset_name = "test",
                asset_code = 1,
                //address = "18PvP5AAUuQUubYd1EXravq3hxJ7ShrMBw",
                usn = guid
                //private_key = "L5cNA52BuPQae88A11T8Jknd1rQqerzaFZ8mqLArygiai6DRLz26"
            };
            if(recommend==null)
            {
                DataService.Transcation(tc =>
                {
                    var uid= tc.CommandSet<userEntity>().Insert(user,true);
                    userbalance.uid = uid;
                    tc.CommandSet<userbalanceEntity>().Insert(userbalance);
                    tc.CommandSet<userextendEntity>().Insert(new userextendEntity
                    {
                        uid=uid,
                        usn=guid,
                        level=1
                    });
                    tc.CommandSet<walletEntity>().Insert(new walletEntity
                    {
                        uid = uid,
                        usn = guid,
                        flash_rate = 0
                    });
                });
            }
            else
            {
                var list = _userService.GetExtendListByUid(recommend.uid);
                DataService.Transcation(tc =>
                {
                    var uid= tc.CommandSet<userEntity>().Insert(user,true);
                    userbalance.uid = uid;
                    tc.CommandSet<userbalanceEntity>().Insert(userbalance);
                    tc.CommandSet<userlevelEntity>().Insert(new userlevelEntity
                    {
                        uid=uid,
                        parent=recommend.uid,
                        times=times
                    });
                    tc.CommandSet<levelextendEntity>().Insert(new levelextendEntity
                    {
                        level=1,
                        parent=recommend.uid,
                        uid=uid,
                        times=times
                    });
                    tc.CommandSet<userextendEntity>().Insert(new userextendEntity
                    {
                        uid = uid,
                        usn = guid,
                        level=1,
                        parent=recommend.uid
                    });
                    tc.CommandSet<walletEntity>().Insert(new walletEntity
                    {
                        uid=uid,
                        usn=guid,
                        flash_rate=0
                    });
                    foreach (var item in list)
                    {
                        tc.CommandSet<levelextendEntity>().Insert(new levelextendEntity
                        {
                            level = item.level + 1,
                            parent=item.parent,
                            uid=uid,
                            times=times
                        });
                    }
                });
            }
            return guid;
        }
        
        public string NRegUser(string phonearea,string phone,userEntity recommend=null)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var times = DateTime.Now.GetTimeUnixLocal();
            var recommendnum = _userService.GetRecommendNum();
            //phonearea = StringHelper.GetNumberFromStr(phonearea);
            var subPhone = phone.Length > 4 ? phone.Substring(phone.Length - 4, 4) : phone;
            var username = $"用户{subPhone}";

            var user = new userEntity
            {
                username = username,
                guid = guid,
                phone = phone,
                phonearea = phonearea,
                times = times,
                recommend = recommendnum,
                origin=-1
            };
            var userbalance = new userbalanceEntity
            {
                asset_code = 1,
                usn = guid
            };
            if (recommend == null)
            {
                DataService.Transcation(tc =>
                {
                    var uid = tc.CommandSet<userEntity>().Insert(user, true);
                    userbalance.uid = uid;
                    tc.CommandSet<userbalanceEntity>().Insert(userbalance);
                    tc.CommandSet<userextendEntity>().Insert(new userextendEntity
                    {
                        uid = uid,
                        usn = guid,
                        level = 1
                    });
                    tc.CommandSet<walletEntity>().Insert(new walletEntity
                    {
                        uid = uid,
                        usn = guid,
                        flash_rate = 100
                    });
                });
            }
            else
            {
                var list = _userService.GetExtendListByUid(recommend.uid);
                DataService.Transcation(tc =>
                {
                    user.origin = recommend.uid;
                    var uid = tc.CommandSet<userEntity>().Insert(user, true);
                    userbalance.uid = uid;
                    tc.CommandSet<userbalanceEntity>().Insert(userbalance);
                    tc.CommandSet<userlevelEntity>().Insert(new userlevelEntity
                    {
                        uid = uid,
                        parent = recommend.uid,
                        times = times
                    });
                    tc.CommandSet<levelextendEntity>().Insert(new levelextendEntity
                    {
                        level = 1,
                        parent = recommend.uid,
                        uid = uid,
                        times = times
                    });
                    tc.CommandSet<userextendEntity>().Insert(new userextendEntity
                    {
                        uid = uid,
                        usn = guid,
                        level = 1,
                        parent = recommend.uid
                    });
                    tc.CommandSet<walletEntity>().Insert(new walletEntity
                    {
                        uid = uid,
                        usn = guid,
                        flash_rate = 100
                    });
                    foreach (var item in list)
                    {
                        tc.CommandSet<levelextendEntity>().Insert(new levelextendEntity
                        {
                            level = item.level + 1,
                            parent = item.parent,
                            uid = uid,
                            times = times
                        });
                    }
                });
            }
            return guid;
        }
    }
}
