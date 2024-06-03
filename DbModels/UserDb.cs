using APIOffice;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pdd_Models.Models;
using Pdd_Models;
using System.Data;

namespace WebGoodApi.DbModels
{
    public class UserDb
    {
        public BackMsg Bslogin([FromBody] LoginUser model)
        {
            BackData backData = new BackData();
            try
            {
                System.Data.SqlClient.SqlParameter[] sqlParameter = new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter("@OutMess", System.Data.SqlDbType.NVarChar,50),
                    new System.Data.SqlClient.SqlParameter("@OutCode", System.Data.SqlDbType.Int),
                    new System.Data.SqlClient.SqlParameter("@UserName", model.user_name),
                    new System.Data.SqlClient.SqlParameter("@Psw", model.user_psw),
                    new System.Data.SqlClient.SqlParameter("@Expire", model.user_expire),
                    new System.Data.SqlClient.SqlParameter("@Phone", model.user_Phone),
                };
                sqlParameter[0].Direction = System.Data.ParameterDirection.Output;
                sqlParameter[1].Direction = System.Data.ParameterDirection.Output;
                DataTable Dt = SQLHelper.ExecuteDataSetProducts("[dbo].[BS_LoginUser]", sqlParameter).Tables[0];
                List<object> ts = new List<object>();
                if (Dt.Rows.Count > 0)
                {
                    LoginUser user = new LoginUser();
                    user.UserId = MyConvert.ToInt(Dt.Rows[0]["user_id"]);
                    user.user_name = Dt.Rows[0]["user_name"].ToString();
                    user.user_desc = Dt.Rows[0]["user_desc"].ToString();
                    user.user_expire = Dt.Rows[0]["user_expire"].ToString();
                    user.user_Phone = Dt.Rows[0]["user_Phone"].ToString();
                    ts.Add(user);
                    backData.Code = 0;
                    backData.Data = ts;
                }
            }
            catch (Exception ex)
            {
                backData.Code = 100;
            }
            finally { }
            BackMsg backMsg = new BackMsg();
            backMsg.Code=backData.Code;
            backMsg.Mess = JsonConvert.SerializeObject(backData.Data);
            return backMsg;
        }
    }
}
