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
    [Description("获取specID")]
    [Route("api/specID")]
    public class GoodSkuSpecController : Controller
    {
        [HttpPost("get")]
        public BackMsg get(string Parent_Spec_Id,string Spec_Name,string access_token)
        {
            BackMsg backMsg = new BackMsg();
            try
            {
                //数据正常请求接口
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                { "type", "pdd.goods.spec.id.get" },
                { "client_id", apiHelp.client_id },
                { "access_token", access_token },
                { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                { "data_type", "JSON" },
                { "parent_spec_id",Parent_Spec_Id},
                {"spec_name",Spec_Name}
                };

                backMsg = apiHelp.SendPddApi(parameters);
                //数据处理(成功时)
                if (backMsg.Code == 0)
                {
                    JToken jToken = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                    backMsg.Mess = jToken["goods_spec_id_get_response"].ToString();
                }
            }
            catch (Exception ex)
            {
                backMsg.Code = 100;
                backMsg.Mess= ex.Message;
            }
            return backMsg;
        }
    }
}
