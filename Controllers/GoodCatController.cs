using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Pdd_Models.Models;
using Pdd_Models;
using WebGoodApi.Class;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Route("api/cat")]
    public class GoodCatController : Controller
    {
        [HttpPost("get")]
        public BackMsg get([FromBody] RequstCat requstCat)
        {
            BackMsg backMsg = new BackMsg();
            //数据正常请求接口
            Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                { "type", "pdd.goods.authorization.cats" },
                { "client_id", apiHelp.client_id },
                { "access_token", requstCat.mall.mall_token },
                { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                { "data_type", "JSON" },
                { "parent_cat_id",requstCat.Catid.ToString()},
            };

            backMsg = apiHelp.SendPddApi(parameters);
            //数据处理(成功时)
            if (backMsg.Code == 0)
            {
                JToken jToken = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                backMsg.Mess = jToken["goods_auth_cats_get_response"]["goods_cats_list"].ToString();
            }
            return backMsg;
        }

        [HttpPost("rule")]
        public List<BackMsg> getCatRule([FromBody] List<Goods_detailModel> BodyList)
        {
            List<BackMsg> results = new BackMsg[BodyList.Count].ToList();
            try
            {
                Parallel.For(0, BodyList.Count, i =>
                {
                    BackMsg backMsg = new BackMsg();
                    //数据正常请求接口
                    Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                        { "type", "pdd.goods.cat.rule.get" },
                        { "client_id", apiHelp.client_id },
                        { "access_token", BodyList[i].mall.mall_token },
                        { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                        { "data_type", "JSON" },
                        { "cat_id",BodyList[i].cat_id.ToString() },
                        { "goods_id",BodyList[i].goods_id.ToString()}
                        };
                    backMsg = apiHelp.SendPddApi(parameters);
                    if (backMsg.Code == 0)
                    {
                        JToken jToken = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                        backMsg.Mess = jToken["cat_rule_get_response"]["goods_properties_rule"]["properties"].ToString();//商品属性信息
                    }
                    results[i] = backMsg;
                });
            }
            catch (Exception ex)
            {
            }
            return results;
        }
    }
}
