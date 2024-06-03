using APIOffice;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdd_Models;
using Pdd_Models.Models;
using System.Data;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Route("api/mall")]
    public class MallController : Controller
    {
        [HttpPost("List")]
        public BackData List([FromBody] MallRequest model)
        {
            BackData backData = new BackData();
            try
            {
                string sql = "select * from u_mall where mall_del=0 and user_id=" + model.user_id ;
                if (!string.IsNullOrEmpty(model.mall_name))
                {
                    sql+= " and mall_name like '%" + model.mall_name + "%'";
                }
                if (model.mall_group != 0)
                {
                    sql += " and "+model.mall_group+ " in ( select * from fn_Split(mall_group,',')) ";
                }
                sql += "order by id desc";
                DataTable dt = SQLHelper.GetDataTable1(sql);
                List<object> list = new List<object>();
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Mallinfo mallinfo = new Mallinfo();
                        mallinfo.mall_desc = dt.Rows[i]["mall_desc"].ToString();
                        mallinfo.merchant_type = Convert.ToInt32(dt.Rows[i]["merchant_type"]);
                        mallinfo.mall_id = Convert.ToInt64(dt.Rows[i]["mall_id"]);
                        mallinfo.logo = dt.Rows[i]["logo"].ToString();
                        mallinfo.mall_name = dt.Rows[i]["mall_name"].ToString();
                        mallinfo.mall_character = Convert.ToInt32(dt.Rows[i]["mall_character"].ToString());

                        mallinfo.mall_token = dt.Rows[i]["mall_token"].ToString();
                        mallinfo.mall_token_expire = MyConvert.ToLong(dt.Rows[i]["mall_token_expire"]);
                        mallinfo.user_id = model.user_id;
                        mallinfo.id = MyConvert.ToInt(dt.Rows[i]["id"]);
                        mallinfo.mall_group = dt.Rows[i]["mall_group"].ToString();
                        list.Add(mallinfo);
                    }
                }
                backData.Code = 0;
                backData.Data = list;
                backData.Mess=dt.Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                backData.Code = 100;
                backData.Mess = ex.Message;
            }
            return backData;
        }

        [HttpPost("remove")]
        public BackMsg remove([FromBody] MallRequest model)
        {
            BackMsg backMsg = new BackMsg();
            try
            {
                backMsg.Mess = SQLHelper.ExecuteCommand1("update u_mall set mall_del=1 where id in( " + model.Ids + " )");
                backMsg.Code = 0;
            }
            catch (Exception ex)
            {
                backMsg.Code = 100;
                backMsg.Mess = ex.Message;
            }
            return backMsg;
        }
    }

    /// <summary>
    /// 店铺请求体
    /// </summary>
    public class MallRequest
    {
        public int user_id {  get; set; }=0;
        public string mall_name { get; set; } = "";

        public string Ids { get; set; } = "";

        public int mall_group {  get; set; } = 0;
    }

}
