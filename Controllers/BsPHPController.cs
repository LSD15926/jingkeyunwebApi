using BsPhpHelper;
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
    [Description("登录")]
    [Route("api/bsPhp")]
    public class BsPHPController : Controller
    {
        [HttpPost("login")]
        public BackMsg login([FromBody] LoginnRequest request)
        {
            BackMsg backMsg = new BackMsg();
            BsPhp bsPhp = new BsPhp();
            backMsg = bsPhp.AppEn_LogIn(request.user, request.pwd, "null", request.key);
            if (backMsg.Mess.Contains("1011"))
            {
                //获取用户基本信息
                backMsg = bsPhp.AppEn_GetUserInfo("UserName,UserMobile,UserVipDate");
                if (backMsg.Code == 0)
                {
                    try
                    {
                        List<string> info = new List<string>();
                        info = backMsg.Mess.Split(',').ToList();
                        if (info.Count != 3)
                        {
                            backMsg.Code = 100;
                            backMsg.Mess = "获取用户信息失败";
                            return backMsg;
                        }
                        LoginUser loginUser = new LoginUser();
                        loginUser.user_name = info[0];
                        loginUser.user_Phone = info[1];
                        loginUser.user_expire = MyConvert.ToTimeStamp(info[2]).ToString();
                        loginUser.user_psw = request.pwd;
                        //数据库二次验证
                        backMsg = new UserDb().Bslogin(loginUser);
                    }
                    catch (Exception ex)
                    {
                        backMsg.Code = 100;
                        backMsg.Mess = "获取用户信息失败！" + ex.Message;
                    }
                }

            }
            else if (backMsg.Mess.Contains("9908"))
            {
                backMsg.Code = 100;
                backMsg.Mess = "你的帐号已经到期了哦,请续费";
            }
            else
            {
                backMsg.Code = 100;
            }

            return backMsg;
        }

        [HttpPost("unbundling")]
        public BackMsg Unbundling([FromBody] LoginnRequest request)
        {
            BackMsg backMsg = new BackMsg();
            backMsg = new BsPhp().AppEn_Unbundling(request.user, request.pwd);
            if (backMsg.Code == 0)
            {
                //解绑成功！
                if (backMsg.Mess.Contains("解除绑定成功"))
                {
                    backMsg.Mess = "解除绑定成功!";
                }
                else
                {
                    backMsg.Code = 100;
                }
            }
            return backMsg;
        }
        [HttpPost("vip")]
        public BackMsg vip([FromBody] LoginnRequest request)
        {
            BackMsg backMsg = new BackMsg();
            if (request.pwd == "")
            {
                backMsg = new BsPhp().AppEn_vipchong(request.user, "", "0", request.vipCode, "");
            }
            else
            {
                backMsg = new BsPhp().AppEn_vipchong(request.user, request.pwd, "1", request.vipCode, "");
            }
            if (backMsg.Code == 0)
            {
                //解绑成功！
                if (backMsg.Mess.Contains("激活成功"))
                {
                    backMsg.Mess = "激活成功!";
                }
                else
                {
                    backMsg.Code = 100;
                }
            }
            return backMsg;
        }
        [HttpPost("regist")]
        public BackMsg register([FromBody] LoginnRequest request)
        {
            BackMsg backMsg = new BackMsg();
            backMsg = new BsPhp().AppEn_registration(request.user, request.pwd, request.pwd, "", "", "", "", request.phone, "");
            if (backMsg.Code == 0)
            {
                //解绑成功！
                if (backMsg.Mess.Contains("注册成功"))
                {
                    backMsg = new BsPhp().AppEn_vipchong(request.user, request.pwd, "1", request.vipCode, "");
                    if (backMsg.Code == 0)
                    {
                        //解绑成功！
                        if (backMsg.Mess.Contains("激活成功"))
                        {
                            backMsg.Mess = "注册成功!";
                        }
                        else
                        {
                            backMsg.Code = 100;
                            backMsg.Mess = "账号注册成功，但充值卡激活失败：" + backMsg.Mess;
                        }
                    }
                }
                else
                {
                    backMsg.Code = 100;
                }
            }
            return backMsg;
        }
    }
    public class LoginnRequest
    {
        public string user { get; set; } = "";
        public string pwd { get; set; } = "";
        public string key { get; set; } = "";
        public string phone { get; set; } = "";

        public string vipCode { get; set; } = "";
    }
}
