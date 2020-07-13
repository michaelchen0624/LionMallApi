using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lion.ViewModel.requestModel;
using Lion.ViewModel.respModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LionGateWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayController : ControllerBase
    {
        [HttpPost("unifiedorder")]
        public ActionResult<RespData<respPrepay>> unifiedorder(reqUnifiedOrder data)
        {
            var paydata = new PayData<reqUnifiedOrder>(data);
            if(paydata.CheckSign(SignType.MD5, "key"))
            {

            }
            else//验签不通过
            {

            }
            return geneRetData.geneRate<respPrepay>(1, null);
        }
    }
}