using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.Services;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InviteController : BaseApiController
    {
        private UserService _userService;
        private IConfiguration _config;
        /// <summary>
        /// 
        /// </summary>
        public InviteController(UserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }
        /// <summary>
        /// 邀请好友界面
        /// </summary>
        /// <returns></returns>
        [HttpPost("InviteView")]
        public ActionResult<RespData<inviteViewModel>> InviteView()
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var model = new inviteViewModel();
            model.inviteCode = user.recommend;
            var invite = _userService.GetInviteView(user.uid);
            var subcount = invite.Item1;
            var parent = invite.Item2;
            model.direct_count = subcount.direct_count;
            model.indirect_count = subcount.indirect_count;
            model.parent = parent == null ? "无" : parent.username;
            var list = _userService.GetUserExtendListByParent(user.uid, 1, 20);
            var inviteList = new List<directSubItem>();
            foreach(var item in list)
            {
                inviteList.Add(new directSubItem
                {
                    userName = item.username,
                    level = item.level,
                    headImg = string.IsNullOrWhiteSpace(item.iconurl) ? "" : $"http://103.149.92.49:8086{item.iconurl}"
                });
            }
            model.subList = inviteList;
            return geneRetData.geneRate<inviteViewModel>(1, model);
        }
        /// <summary>
        /// 获取直接推荐列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSubItem")]
        public ActionResult<RespData<IList<directSubItem>>> GetSubItem(reqPage data)
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var list= _userService.GetUserExtendListByParent(user.uid,data.pageIndex,20);
            var dlist = new List<directSubItem>();
            foreach(var item in list)
            {
                dlist.Add(new directSubItem
                {
                    userName = item.username,
                    level = item.level,
                    phone=item.phone,
                    phonearea=item.phonearea,
                    headImg = string.IsNullOrWhiteSpace(item.iconurl) ? "" : $"http://103.149.92.49:8086{item.iconurl}"
                });
            }
            return geneRetData.geneRate<IList<directSubItem>>(1, dlist);
        }

        /// <summary>
        /// 获取间接推荐列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetSubTwoItem")]
        public ActionResult<RespData<IList<directSubItem>>> GetSubTwoItem(reqPage data)
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var list = _userService.GetUserExtendTwoListByParent(user.uid, data.pageIndex, 20);
            var dlist = new List<directSubItem>();
            foreach (var item in list)
            {
                dlist.Add(new directSubItem
                {
                    userName = item.username,
                    level = item.level,
                    headImg = string.IsNullOrWhiteSpace(item.iconurl) ? "" : $"http://103.149.92.49:8086{item.iconurl}"
                });
            }
            return geneRetData.geneRate<IList<directSubItem>>(1, dlist);
        }

        /// <summary>
        /// 获取推荐二维码
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetQrCode")]
        public ActionResult<RespData<respInviteUrl>> GetQrCode()
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var url = _config.GetValue<string>("OrderConfig:regUrl");
            return geneRetData.geneRate<respInviteUrl>(1, new respInviteUrl
            {
                qrCode = $"{url}/{user.recommend}"
            });
        }
    }
}