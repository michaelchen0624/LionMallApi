using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.Services;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prod.Core;

namespace LionMallApi.Controllers
{
    /// <summary>
    /// 收款方式
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PayMentController : BaseApiController
    {
        private UserService _userService;
        private LangService _langService;
        private PaymentService _paymentService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="paymentService"></param>
        /// <param name="langService"></param>
        public PayMentController(UserService userService,PaymentService paymentService,LangService langService)
        {
            _userService = userService;
            _paymentService = paymentService;
            _langService = langService;
        }
        /// <summary>
        /// 用户添加收款方式类型选择
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetPayMentListView")]
        public ActionResult<RespData<payMentView>> GetPayMentListView()
        {
            //var user = _userService.GetUserByUserSN(ticket.userId);
            //var upayment = _changerService.GetUPayMentListByUid(user.uid);
            var payment = _paymentService.GetPayMentList();
            var paymentview = new payMentView();
            var list = new List<payMentTitle>();
            foreach (var item in payment)
            {
                list.Add(new payMentTitle
                {
                    icon = item.icon,
                    payname = item.pay_name,
                    paysn=item.guid,
                    paytitle= lang == "cn" ? item.pay_cnname : item.pay_name,
                });
            }
            paymentview.list = list;
            return geneRetData.geneRate<payMentView>(1, paymentview);
        }
        /// <summary>
        /// 用户收款方式列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("UPayMentView")]
        public ActionResult<RespData<upayMentView>> UPayMentView()
        {
            var user = _userService.GetUserByUsnNotCache(ticket.userId);
            var list= _paymentService.GetUPayMentListByUid(user.uid);
            var payment = new upayMentView();
            payment.list = new List<upayMentTitle>();
            foreach (var item in list)
            {
                payment.list.Add(new upayMentTitle
                {
                    icon = item.icon,
                    account = item.account,
                    active = user.d_payment == item.id ? 1 : 0,
                    payname = item.pay_name,
                    paysn = item.guid,
                    paytitle = lang == "cn" ? item.pay_cnname : item.pay_name
                });
            }
            return geneRetData.geneRate<upayMentView>(1, payment);
        }
        /// <summary>
        /// 用户增加收款方式界面 字段根据返回生成的qrcode=1需要传二维码的
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddUPayMentView")]
        public ActionResult<RespData<addUPayMentView>> AddUPayMentView(paySN data)
        {
            var payment = _paymentService.GetPayMentList().Where(o => o.guid == data.paysn).FirstOrDefault();
            var view = new addUPayMentView();
            var list = _paymentService.GetPayMentFieldByPid(payment.id,lang);
            view.contentList = new List<addUPayContent>();
            foreach (var item in list)
            {
                view.contentList.Add(new addUPayContent
                {
                    fieldtip=item.fieldtip,
                    qrcode=item.imgflag,
                    submitTitle=item.fieldname,
                    title=item.fieldtitle,
                    digFlag=item.digflag,
                    fieldsort=item.fieldsort
                });
            }
            var paytitle = lang == "cn" ? payment.pay_cnname : payment.pay_name;
            view.title = $"{_langService.GetLangByTitle("添加", lang)}{paytitle}";
            view.paysn = payment.guid;
            return geneRetData.geneRate<addUPayMentView>(1,view);
        }
        /// <summary>
        /// 增加收款方式提交
        /// </summary>
        /// <returns></returns>
        [HttpPost("addUPayMentSubmit")]
        public ActionResult<RespData<string>> addUPayMentSubmit(addPaymentData data)
        {
            var payment = _paymentService.GetPayMentList().Where(o => o.guid == data.paysn).FirstOrDefault();
            var user = _userService.GetUserByUserSN(ticket.userId);
            var count= _paymentService.InsertPayMent(new Lion.Entity.upaymentEntity
            {
                payid = payment.id,
                pay_type=payment.pay_type,
                pay_name = payment.pay_name,
                pay_cnname=payment.pay_cnname,
                payee = data.payee,
                qrcode = string.IsNullOrWhiteSpace(data.qrcode) ? "" : data.qrcode,
                account = data.account,
                uid = user.uid,
                bank = string.IsNullOrWhiteSpace(data.bank) ? "" : data.bank,
                icon = string.IsNullOrWhiteSpace(payment.icon) ? "" : payment.icon,
                guid=Guid.NewGuid().ToString().Replace("-",""),
                active=1,
                times = DateTime.Now.GetTimeUnixLocal()
            });
            if (count > 0)
                return geneRetData.geneRate<string>(1, "success");
            else
                return geneRetData.geneRate<string>(1401, "",
                    _langService.GetLangByTitle("保存不成功", lang));
        }
        /// <summary>
        /// 激活收款方式
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetActive")]
        public ActionResult<RespData<string>> SetActive(payActive data)
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var upayment = _paymentService.GetUPaymentByGuid(data.paysn);
            if(upayment==null)
                return geneRetData.geneRate<string>(1401, "",
                    _langService.GetLangByTitle("收款方式不存在", lang));
            var count= _paymentService.SetUPayMentActive(user.uid, upayment.id, 1);
            //var count = _paymentService.SetUPayMentActive(user.uid, data.paysn, data.active);
            if (count > 0)
                return geneRetData.geneRate<string>(1, "success");
            else
                return geneRetData.geneRate<string>(1401, "",
                    _langService.GetLangByTitle("保存不成功", lang));
        }
        /// <summary>
        /// 删除收款方式
        /// </summary>
        /// <returns></returns>
        [HttpPost("PayMentDel")]
        public ActionResult<RespData<string>> PayMentDel(paySN data)
        {
            var user = _userService.GetUserByUserSN(ticket.userId);
            var count = _paymentService.DelUPayMent(user.uid, data.paysn);
            if (count > 0)
                return geneRetData.geneRate<string>(1, "success");
            else
                return geneRetData.geneRate<string>(1401, "",
                    _langService.GetLangByTitle("保存不成功", lang));
        }
    }
}