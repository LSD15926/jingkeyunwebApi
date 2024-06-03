using APIOffice;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pdd_Models.Models;
using Pdd_Models;
using System.Data;
using Newtonsoft.Json.Linq;
using WebGoodApi.Class;

namespace WebGoodApi.DbModels
{
    public static class MallDb
    {
        public static BackMsg upd(Mallinfo model)
        {

            BackMsg backData = new BackMsg();
            try
            {
                DataTable dt = SQLHelper.GetDataTable1("select * from u_mall where user_id=" + model.user_id + " and mall_id=" + model.mall_id);
                if (dt.Rows.Count == 0)//不存在新增
                {
                    backData.Mess = SQLHelper.ExecuteCommand1(string.Format("insert into u_mall(user_id,logo,mall_desc,mall_id,mall_name,merchant_type,mall_character,mall_token,mall_token_expire,mall_group) " +
                        " values({0},'{1}','{2}',{3},'{4}',{5},{6},'{7}',{8},'')", model.user_id, model.logo, model.mall_desc, model.mall_id, model.mall_name, model.merchant_type, model.mall_character
                        , model.mall_token, model.mall_token_expire));
                }
                else
                {
                    backData.Mess = SQLHelper.ExecuteCommand1(string.Format("update u_mall set mall_token='{0}',mall_token_expire={1},mall_del=0,mall_name='{2}',mall_group='{5}' where mall_id={4} "//user_id={3} and 
                        , model.mall_token, model.mall_token_expire, model.mall_name, model.user_id, model.mall_id, model.mall_group));
                    SQLHelper.ExecuteCommand1("delete good_list where mall_id="+model.mall_id);//删除旧缓存
                }

                backData.Code = 0;
            }
            catch (Exception ex)
            {
                backData.Code = 100;
                backData.Mess = ex.Message;
            }
            return backData;
        }


        public static BackMsg Info( string Token)
        {
            BackMsg backMsg = new BackMsg();
            //数据正常请求接口
            Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                { "type", "pdd.mall.info.get" },
                { "client_id", apiHelp.client_id },
                { "access_token", Token },
                { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                { "data_type", "JSON" },
            };

            backMsg = apiHelp.SendPddApi(parameters);
            //数据处理(成功时)
            if (backMsg.Code == 0)
            {
                JToken jToken = JsonConvert.DeserializeObject<JToken>(backMsg.Mess);
                backMsg.Mess = jToken["mall_info_get_response"].ToString();
            }
            return backMsg;
        }

    }
}
