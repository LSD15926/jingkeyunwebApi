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
    [Route("api/quan")]
    public class GoodQuantityController : Controller
    {
        [HttpPost("many")]
        public BackMsg more([FromBody] List<requestQuantity> BodyList)
        {
            BackMsg back=new BackMsg();
            back.Code = 0;
            try
            {
                List<BackMsg> results = new BackMsg[BodyList.Count].ToList();
                Parallel.For(0, BodyList.Count, i =>
                {
                    BackMsg backMsg = new BackMsg();
                    //数据正常请求接口
                    Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                        { "type", "pdd.goods.quantity.update" },
                        { "client_id", apiHelp.client_id },
                        { "access_token", BodyList[i].Malls.mall_token },
                        { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                        { "data_type", "JSON" },
                        { "goods_id", BodyList[i].goods_id.ToString() },
                        { "quantity", BodyList[i].quantity.ToString() },
                        { "sku_id", BodyList[i].sku_id.ToString() },
                        { "outer_id", BodyList[i].outer_id.ToString() },
                        { "update_type", BodyList[i]    .update_type.ToString() },
                    };
                    backMsg = apiHelp.SendPddApi(parameters);
                    results[i] = backMsg;
                });

                int ErrorSum = 0;
                for (int i=0;i<results.Count;i++)
                {
                    if (results[i].Code != 0)
                    {
                        ErrorSum++;
                        back.Mess += $"\n【{BodyList[i].goods_id}】:{results[i].Mess}";
                        continue;
                    }
                    //获取修改数据的id
                }

                back.Mess = $"修改完成：商品总数{results.Count},成功{results.Count - ErrorSum}个,失败{ErrorSum}个！"+back.Mess;
            }
            catch (Exception ex)
            {
                back.Code = 100;
                back.Mess=ex.Message;
            }
            return back;
        }
    
       
        
    
    }
}
