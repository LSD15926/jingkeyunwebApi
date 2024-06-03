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
    [Route("api/sale")]
    public class GoodSaleController : Controller
    {
        [HttpPost("one")]
        public BackMsg one([FromBody] requsetSaleBody request)
        {
            BackMsg backMsg = new BackMsg();
            try {
                //数据正常请求接口
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "type", "pdd.goods.sale.status.set" },
                    { "client_id", apiHelp.client_id },
                    { "access_token", request.Malls.mall_token },
                    { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                    { "data_type", "JSON" },
                    { "goods_id", request.goods_id.ToString() },
                    { "is_onsale", request.is_onsale.ToString() },

                };
                backMsg = apiHelp.SendPddApi(parameters);
                if (backMsg.Code == 0)
                {
                    //修改数据库
                    SQLHelper.ExecuteCommand1("update good_list set is_onsale="+ request.is_onsale + " where mall_id="+ request.Malls.mall_id+ " and goods_id="+ request.goods_id );
                }
            }
            catch (Exception ex)
            {
                backMsg.Code = 100;
                backMsg.Mess=ex.Message;
            }
            return backMsg;
        }

        [HttpPost("many")]
        public BackMsg more([FromBody] List<requsetSaleBody> BodyList)
        {
            BackMsg back=new BackMsg();
            back.Code = 0;
            try
            {
                List<BackMsg> results = new BackMsg[BodyList.Count].ToList();
                Parallel.For(0, BodyList.Count, i =>
                {
                    BackMsg backMsg = new BackMsg();
                    if (BodyList[i].goods_id == 0 || BodyList[i].is_onsale == -1)
                    {
                        backMsg.Code = 1001;
                        backMsg.Mess = "参数错误缺少参数。";
                        results[i] = backMsg;
                    }
                    else
                    {
                        //数据正常请求接口
                        Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                        { "type", "pdd.goods.sale.status.set" },
                        { "client_id", apiHelp.client_id },
                        { "access_token", BodyList[i].Malls.mall_token },
                        { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                        { "data_type", "JSON" },
                        { "goods_id", BodyList[i].goods_id.ToString() },
                        { "is_onsale", BodyList[i].is_onsale.ToString() },

                    };
                        backMsg = apiHelp.SendPddApi(parameters);
                        results[i] = backMsg;
                    }
                });
                int ErrorSum = 0;
                List<string> goodIds = new List<string>();
                for (int i=0;i<results.Count;i++)
                {
                    if (results[i].Code != 0)
                    {
                        ErrorSum++;
                        back.Mess += $"\n【{BodyList[i].goods_id}】:{results[i].Mess}";
                        continue;
                    }
                    //获取修改数据的id
                    goodIds.Add(BodyList[i].goods_id.ToString());

                }
                string sql = "update good_list set is_onsale="+ BodyList[0].is_onsale+" where goods_id in ("+ string.Join(',' ,goodIds)+ ")";
                string row=SQLHelper.ExecuteCommand1(sql);//执行修改
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
