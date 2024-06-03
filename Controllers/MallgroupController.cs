using APIOffice;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pdd_Models;
using Pdd_Models.Models;
using System.ComponentModel;
using System.Data;
using WebGoodApi.DbModels;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Description("获取店铺分组")]
    [Route("api/group")]
    public class MallgroupController : Controller
    {
        [HttpPost("list")]
        public BackData list([FromBody] MallGroup group)
        {

            BackData backData = new BackData();
            try
            {
                DataTable dt = SQLHelper.GetDataTable1("select * from u_mall_group where  group_user=" + group.group_user);
                List<object> list = new List<object>();
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        MallGroup model = new MallGroup();
                        model.group_id = MyConvert.ToInt(dt.Rows[i]["group_id"]);
                        model.group_name = dt.Rows[i]["group_name"].ToString();
                        model.group_user = MyConvert.ToInt(dt.Rows[i]["group_user"]);
                        model.group_notes = dt.Rows[i]["group_notes"].ToString();
                        list.Add(model);
                    }
                }
                backData.Code = 0;
                backData.Data = list;
            }
            catch(Exception ex)
            {
                backData.Code = 100;
                backData.Mess=ex.Message;
            }
            return backData;
        }

        [HttpPost("add")]
        public BackMsg Add([FromBody] requestGroup model)
        {
            BackMsg backData = new BackMsg();
            try
            {
                System.Data.SqlClient.SqlParameter[] sqlParameter = new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter("@OutMess", System.Data.SqlDbType.NVarChar,50),
                    new System.Data.SqlClient.SqlParameter("@OutCode", System.Data.SqlDbType.Int),
                    new System.Data.SqlClient.SqlParameter("@group_name", model.group_name),
                    new System.Data.SqlClient.SqlParameter("@group_notes", model.group_notes),
                    new System.Data.SqlClient.SqlParameter("@group_user", model.group_user),
                };
                sqlParameter[0].Direction = System.Data.ParameterDirection.Output;
                sqlParameter[1].Direction = System.Data.ParameterDirection.Output;
                SQLHelper.ExecuteDataSetProducts("[dbo].[MallGroupAdd]", sqlParameter);
                backData.Code = MyConvert.ToInt(sqlParameter[1].Value);
                backData.Mess = sqlParameter[0].Value.ToString();

                if (backData.Code == 0)
                {
                    List<string> mallId=new List<string>();
                    //修改店铺分组
                    foreach (var item in model.mallinfos)
                    {
                        mallId.Add(item.id.ToString());
                    }
                    string Sql = $"update u_mall set mall_group=(case mall_group when '' then  '{backData.Mess}' else mall_group+',{backData.Mess}' end)  where id in (" + string.Join(",", mallId) + ")";
                    SQLHelper.ExecuteCommand1(Sql);
                }
            }
            catch (Exception ex)
            {
                backData.Code = 100;
                backData.Mess=ex.Message;
            }
            finally { }
            return backData;
        }

        [HttpPost("del")]
        public BackMsg Del([FromBody] List<MallGroup> models)
        {
            BackMsg backData = new BackMsg();
            try
            {
                List<string> groups = new List<string>();
                foreach (var item in models)
                {
                    groups.Add(item.group_id.ToString());

                }
                backData.Mess = SQLHelper.ExecuteCommand1(string.Format("delete u_mall_group  where group_id in ({0}) "
                    , string.Join(",",groups)));

                backData.Code = 0;
            }
            catch (Exception ex)
            {
                backData.Code = 100;
                backData.Mess = ex.Message;
            }
            return backData;
        }


        [HttpPost("edit")]
        public BackMsg edit([FromBody] requestGroup model)
        {
            BackMsg backData = new BackMsg();
            try
            {
                //1.获取所有分组下的店铺
                DataTable dt = SQLHelper.GetDataTable1("select * from u_mall where "+model.group_id+" in ( select * from fn_Split(mall_group,','))");
                foreach (DataRow dr in dt.Rows)
                {
                    List<string> groups=new List<string>();
                    groups = dr["mall_group"].ToString().Split(',').ToList();
                    groups.Remove(model.group_id.ToString());
                    SQLHelper.ExecuteCommand1("update u_mall set mall_group='"+ string.Join(",", groups) + "' where id=" + dr["id"].ToString());
                }

                List<string> mallId = new List<string>();
                //修改店铺分组
                foreach (var item in model.mallinfos)
                {
                    mallId.Add(item.id.ToString());
                }
                if (mallId.Count > 0)
                {
                    string Sql = $"update u_mall set mall_group=(case mall_group when '' then  '{model.group_id}' else mall_group+',{model.group_id}' end)  where id in (" + string.Join(",", mallId) + ")";
                    SQLHelper.ExecuteCommand1(Sql);
                }
            }
            catch (Exception ex)
            {
                backData.Code = 100;
                backData.Mess = ex.Message;
            }
            finally { }
            return backData;
        }

    }

    public class requestGroup
    {
        public string group_name { get; set; } = "";
        public string group_notes { get; set; } = "";
        public int group_user { get; set; } = 0;
        public int group_id { get; set; } = 0;

        public List<Mallinfo> mallinfos { get; set; }= new List<Mallinfo>();
    }
}
