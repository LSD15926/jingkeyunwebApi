using APIOffice;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pdd_Models;
using Pdd_Models.Models;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using WebGoodApi.Class;
using WebGoodApi.DbModels;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Description("获取token")]
    [Route("api/template")]
    public class GoodTempController : Controller
    {
        [HttpPost("list")]
        public BackMsg list([FromBody] TempRequset requset)
        {
            BackMsg backMsg = new BackMsg();
            try {
                //数据正常请求接口
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                { "type", "pdd.goods.logistics.template.get" },
                { "client_id", apiHelp.client_id },
                { "access_token", requset.mall.mall_token },
                { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                { "data_type", "JSON" },
                };
                backMsg = apiHelp.SendPddApi(parameters);
            }
            catch (Exception ex)
            {
                backMsg.Code = 100;
                backMsg.Mess=ex.Message;
            }
            return backMsg;
        }
    }
}
