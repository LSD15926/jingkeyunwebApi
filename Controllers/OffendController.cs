using APIOffice;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pdd_Models;
using Pdd_Models.Models;
using System.Data;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Route("api/offend")]
    public class OffendController : Controller
    {
        [HttpPost("List")]
        public BackData List([FromBody] requestOffend request)
        {
            BackData backData = new BackData();
            try
            {
                string where = "";
                if (request.word != "")
                {
                    where = " and Offend_Word like '%" + request.word + "%'";
                }
                System.Data.SqlClient.SqlParameter[] sqlParameter = new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter("@OutMess", System.Data.SqlDbType.NVarChar,50),
                    new System.Data.SqlClient.SqlParameter("@OutCode", System.Data.SqlDbType.Int),
                    new System.Data.SqlClient.SqlParameter("@TotalCount", System.Data.SqlDbType.Int),

                    new System.Data.SqlClient.SqlParameter("@PageWhere",where),
                    new System.Data.SqlClient.SqlParameter("@PageIndex", request.page),
                    new System.Data.SqlClient.SqlParameter("@PageSize", request.pageSize),
                    };
                sqlParameter[0].Direction = System.Data.ParameterDirection.Output;
                sqlParameter[1].Direction = System.Data.ParameterDirection.Output;
                sqlParameter[2].Direction = System.Data.ParameterDirection.Output;
                DataTable Dt = SQLHelper.ExecuteDataSetProducts("[dbo].[Offend_Response_List]", sqlParameter).Tables[0];
                List<object> models = new List<object>();
                foreach (DataRow dr in Dt.Rows)
                {
                    models.Add(dr["Offend_Word"].ToString());
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

        [HttpPost("add")]
        public BackMsg add([FromBody] requestOffend model)
        {
            BackMsg backMsg = new BackMsg();
            try
            {
                backMsg.Mess = SQLHelper.ExecuteCommand1("insert into OffendWord (Offend_Word) values('"+model.word+"')");
                backMsg.Code = 0;
            }
            catch (Exception ex)
            {
                backMsg.Code = 100;
                backMsg.Mess = ex.Message;
            }
            return backMsg;
        }
        [HttpPost("del")]
        public BackMsg del([FromBody] requestOffend model)
        {
            BackMsg backMsg = new BackMsg();
            try
            {
                backMsg.Mess = SQLHelper.ExecuteCommand1("delete OffendWord where Offend_Word='"+model.word+"'");
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

    public class requestOffend
    {
        public string word { get; set; } = "";
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}
