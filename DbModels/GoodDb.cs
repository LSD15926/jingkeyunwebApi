using APIOffice;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pdd_Models.Models;
using Pdd_Models;
using System.Data;
using WebGoodApi.Class;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Numerics;
using WebGoodApi.Controllers;
using System.Reflection;

namespace WebGoodApi.DbModels
{
    public  class GoodDb
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  int down(requestGoodList BodyList)
        {
            int total = 0;
            BackMsg backMsg = new BackMsg();
            Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "type", "pdd.goods.list.get" },
                    { "client_id", apiHelp.client_id },
                    { "access_token", BodyList.Malls.mall_token },
                    { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                    { "data_type", "JSON" },
                    { "page",BodyList.page.ToString()},
                    { "page_size",BodyList.page_size.ToString() }

                };
            backMsg = apiHelp.SendPddApi(parameters);
            if (backMsg.Code == 0)
            {
                JToken jToken = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                var json1 = jToken["goods_list_get_response"]["goods_list"].ToString();
                List<GoodListResponse> pageGood = JsonConvert.DeserializeObject<List<GoodListResponse>>(json1);
                //保存到数据库
                DataTable dt = GetTableSchema();
                foreach (var good in pageGood)
                {


                    dt.Rows.Add(good.goods_id,good.goods_name,good.goods_quantity,0,good.image_url,good.thumb_url,JsonConvert.SerializeObject(good.sku_list),JsonConvert.SerializeObject(BodyList.Malls),0,good.is_onsale,BodyList.Malls.mall_id);
                }
                SQLHelper.BulkToDB("good_list", dt);
                //获取总数
                total = MyConvert.ToInt(jToken["goods_list_get_response"]["total_count"]);

            }
            else
            {
                total = -1;
            }
            return total;
        }

        public  DataTable GetTableSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]{
                //new DataColumn("id",typeof(int)),
                new DataColumn("goods_id",typeof(long)),
                new DataColumn("goods_name",typeof(string)),
                new DataColumn("goods_quantity",typeof(long)),
                new DataColumn("goods_reserve_quantity",typeof(long)),
                new DataColumn("image_url",typeof(string)),
                new DataColumn("thumb_url",typeof(string)),
                new DataColumn("sku_list",typeof(string)),
                new DataColumn("Mallinfo",typeof(string)),
                new DataColumn("is_more_sku",typeof(int)),
                new DataColumn("is_onsale",typeof(int)),
                new DataColumn("mall_id",typeof(int)),
                });
            return  dt;
        }


        public BackData List(GoodRequest request, List<string> mallIds)
        {
            BackData backData = new BackData();
            string sqlQuery = " ";
            if (mallIds.Count > 0)
            {
                sqlQuery += " and mall_id in (" + string.Join(',', mallIds) + ")";
            }
            if (request.good_ids != "")
            {

                if (request.good_ids.Contains("\n"))
                {
                    request.good_ids = request.good_ids.Replace("\n", ",");
                }
                else if (request.good_ids.Contains(" "))
                {
                    request.good_ids = request.good_ids.Replace(" ", ",");
                }
                sqlQuery += " and goods_id in (" + request.good_ids + ")";
            }
            if (request.good_name != "")
            {
                sqlQuery += " and goods_name like '%" + request.good_name + "%'";
            }
            if (request.isOnsale != -1)
            {
                sqlQuery += " and is_onsale=" + request.isOnsale;
            }
            if (request.good_quantity != "")
            {
                sqlQuery += request.good_quantity;
            }
            if (request.isOffend)
            {
                sqlQuery += " and EXISTS (SELECT 1 FROM OffendWord WHERE goods_name LIKE '%' + Offend_Word + '%')";
            }
            try
            {
                System.Data.SqlClient.SqlParameter[] sqlParameter = new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter("@OutMess", System.Data.SqlDbType.NVarChar,50),
                    new System.Data.SqlClient.SqlParameter("@OutCode", System.Data.SqlDbType.Int),
                    new System.Data.SqlClient.SqlParameter("@TotalCount", System.Data.SqlDbType.Int),

                    new System.Data.SqlClient.SqlParameter("@PageWhere",sqlQuery),
                    new System.Data.SqlClient.SqlParameter("@PageIndex", request.page),
                    new System.Data.SqlClient.SqlParameter("@PageSize", request.page_size),
                    };
                sqlParameter[0].Direction = System.Data.ParameterDirection.Output;
                sqlParameter[1].Direction = System.Data.ParameterDirection.Output;
                sqlParameter[2].Direction = System.Data.ParameterDirection.Output;
                DataTable Dt = SQLHelper.ExecuteDataSetProducts("[dbo].[Good_Response_List]", sqlParameter).Tables[0];
                List<object> models = new List<object>();
                foreach (DataRow dr in Dt.Rows)
                {
                    GoodListResponse good = new GoodListResponse();
                    good.is_onsale = MyConvert.ToInt(dr["is_onsale"]);
                    good.sku_list = JsonConvert.DeserializeObject<List<skuList>>(dr["sku_list"].ToString());
                    good.goods_name = dr["goods_name"].ToString();
                    good.goods_id = MyConvert.ToLong(dr["goods_id"]);
                    good.goods_quantity = MyConvert.ToLong(dr["goods_quantity"]);
                    good.image_url = dr["image_url"].ToString();
                    good.thumb_url = dr["thumb_url"].ToString();
                    good.is_onsale = MyConvert.ToInt(dr["is_onsale"]);
                    good.Mallinfo= JsonConvert.DeserializeObject<Mallinfo>(dr["Mallinfo"].ToString());
                    models.Add(good);
                }
                backData.Code = Convert.ToInt32(sqlParameter[1].Value);
                backData.Data = models;
                backData.Mess = sqlParameter[2].Value.ToString();
            }
            catch (Exception ex)
            {
                backData.Mess = ex.Message;
                backData.Code = 100;
            }
            return backData;
        }


        public Goods_detailModel getmodel(long goodsID, string access_token)
        {
            Goods_detailModel model = new Goods_detailModel();
            //根据商品id获取商品明细
            Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "type", "pdd.goods.detail.get" },
                    { "client_id", apiHelp.client_id },
                    { "access_token", access_token },
                    { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                    { "data_type", "JSON" },
                    { "goods_id", goodsID.ToString() },

                };
            BackMsg backMsg = apiHelp.SendPddApi(parameters);
            if (backMsg.Code == 0)
            {
                JToken token = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                var json = JsonConvert.SerializeObject(token["goods_detail_get_response"]);
                model = JsonConvert.DeserializeObject<Goods_detailModel>(json);
            }

            return model;
        }
    }
}
