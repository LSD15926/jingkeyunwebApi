using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pdd_Models;
using Pdd_Models.Models;
using System.ComponentModel;
using System.Data;
using WebGoodApi.Class;
using WebGoodApi.DbModels;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Description("获取token")]
    [Route("api/token")]
    public class GetTokenController : Controller
    {
        [HttpPost("get")]
        public BackMsg get([FromBody] TokenRequest request)
        {

            BackMsg backMsg = new BackMsg();
            //数据正常请求接口
            Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                { "type", "pdd.pop.auth.token.create" },
                { "client_id", apiHelp.client_id },
                { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                { "data_type", "JSON" },  
                { "code",request.code},
            };

            backMsg = apiHelp.SendPddApi(parameters);
            //数据处理(成功时)
            if (backMsg.Code == 0)
            {
                JToken jToken = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                backMsg = MallDb.Info(jToken["pop_auth_token_create_response"]["access_token"].ToString());

                Mallinfo model = JsonConvert.DeserializeObject<Mallinfo>(backMsg.Mess);

                model.mall_token = jToken["pop_auth_token_create_response"]["access_token"].ToString();
                model.mall_token_expire = MyConvert.ToLong(jToken["pop_auth_token_create_response"]["expires_at"]);
                model.user_id = request.userId;
                //获取店铺信息
                //添加至数据库
                backMsg = MallDb.upd(model);
            }
            return backMsg;
        }

    }
    public class TokenRequest
    {
        public string code { get; set; } = "";
        public int userId {  get; set; } = 0;
    }
}
