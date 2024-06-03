using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Pdd_Models.Models;
using Pdd_Models;
using System.Reflection;
using WebGoodApi.Class;
using WebGoodApi.DbModels;
using APIOffice;
using System.Data;
using jingkeyun.Class;
using System.DrawingCore;

namespace WebGoodApi.Controllers
{
    [ApiController]
    [Route("api/good")]
    public class GoodController : Controller
    {
        [HttpPost("List/get")]
        public BackData get([FromBody] GoodRequest model)
        {
            BackData backData = new BackData();
            GoodDb db = new GoodDb();
            try
            {
                //验证是否缓存
                //遍历获取所有店铺
                List<string> mallIds = new List<string>();
                foreach (var item in model.mallinfos)
                {
                    mallIds.Add(item.mall_id.ToString());
                    string sql = "select count(1) from good_list where mall_id=" + item.mall_id;
                    if (SQLHelper.Exists(sql))
                        continue;
                    //接口下载所有商品并保存
                    //执行第一次接口
                    requestGoodList request = new requestGoodList();
                    request.Malls = item;
                    request.page = 1;
                    request.page_size = 100;
                    int total = db.down(request);
                    total -= 100;
                    int pageIndex = 2;
                    while (total > 0)
                    {
                        request = new requestGoodList();
                        request.Malls = item;
                        request.page = pageIndex;
                        request.page_size = 100;
                        db.down(request);
                        total -= 100;
                        pageIndex++;
                    }

                }
                //根据条件筛选数据库
                backData= db.List(model, mallIds);

            }
            catch (Exception ex)
            {
                backData.Mess=ex.Message;
                backData.Code = 100;
            }

            return backData;
        }

        [HttpPost("detail/get")]
        public BackData List([FromBody] List<RequstGoodDetail> Goods)
        {
            BackData results = new BackData();
            List<Goods_detailModel> models = new Goods_detailModel[Goods.Count].ToList();
            Parallel.For(0, Goods.Count, i =>
            {
                try
                {
                    Goods_detailModel model = new GoodDb().getmodel(Goods[i].Goods, Goods[i].mall.mall_token);
                    model.mall = Goods[i].mall;
                    models[i] = model;
                }
                catch (Exception ex)
                {
                }
            });
            results.Code = 0;
            results.Mess = models.Count.ToString();
            results.Data = new List<object>();
            results.Data.AddRange(models);

            return results;
        }
        
        [HttpPost("detail/update")]
        public BackMsg update([FromBody] List<RequstGoodEditModel> BodyList)
        {
            BackMsg back=new BackMsg();
            List<BackMsg> results = new List<BackMsg>();
            try
            {
                foreach (var item in BodyList)
                {
                    BackMsg backMsg = new BackMsg();
                    if (item.goods_id == 0)
                    {
                        backMsg.Code = 1001;
                        backMsg.Mess = "参数错误缺少参数。";
                        results.Add(backMsg);
                        continue;
                    }
                    Goods_detailModel detaiModel = new GoodDb().getmodel(item.goods_id, item.Malls.mall_token);

                    switch (item.ApiType)
                    {
                        case 0:
                            detaiModel.goods_name = item.goods_name;
                            break;
                        case 1:
                            detaiModel.tiny_name = item.tiny_name;
                            break;
                        case 2:
                            if (item.shipment_limit_second == 0)
                            {
                                detaiModel.shipment_limit_second = 86400;
                                detaiModel.delivery_one_day = 1;
                            }
                            else
                                detaiModel.shipment_limit_second = item.shipment_limit_second;
                            break;
                        case 3:
                            detaiModel.sku_list = item.sku_list;
                            break;
                        case 4:
                            detaiModel.cat_id = item.cat_id;
                            break;
                        case 5:
                            detaiModel.cost_template_id = item.cost_template_id;
                            break;
                        case 6:
                            detaiModel.two_pieces_discount = item.two_pieces_discount;
                            break;
                        case 7:
                            detaiModel.buy_limit = item.buy_limit;
                            break;
                        case 8:
                            detaiModel.order_limit = item.order_limit;
                            break;
                        case 9:
                            detaiModel.is_folt = item.is_folt;
                            break;
                        case 10:
                            detaiModel.bad_fruit_claim = item.bad_fruit_claim;
                            break;
                        case 11:
                            detaiModel.goods_desc = item.goods_desc;
                            break;
                        case 12:// 商品轮播图
                            detaiModel.carousel_gallery_list = item.carousel_gallery;
                            break;
                        case 13:// 商品详情图
                            detaiModel.detail_gallery_list = item.detail_gallery;
                            break;
                        case 14:// 
                            detaiModel.sku_list = item.sku_list;
                            detaiModel.outer_goods_id = item.outer_goods_id;
                            break;
                        case 15:
                            detaiModel.goods_property_list = item.goods_property_list;
                            break;
                        case 16://非预售
                            detaiModel.is_pre_sale = 0;
                            detaiModel.is_group_pre_sale = 0;
                            detaiModel.pre_sale_time = 0;
                            detaiModel.is_sku_pre_sale = 0;
                            if (detaiModel.shipment_limit_second > 172800)
                                detaiModel.shipment_limit_second = 172800;
                            break;
                        case 17:
                            detaiModel.is_pre_sale = 1;
                            detaiModel.is_sku_pre_sale = 0;
                            detaiModel.pre_sale_time = item.pre_sale_time;
                            detaiModel.is_group_pre_sale = 0;
                            break;
                        case 18:
                            detaiModel.is_pre_sale = 0;
                            detaiModel.is_sku_pre_sale = 0;
                            detaiModel.shipment_limit_second = item.shipment_limit_second;
                            detaiModel.pre_sale_time = 0;
                            detaiModel.is_group_pre_sale = 1;
                            break;
                        case 19://规格预售
                            detaiModel.is_pre_sale = 0;
                            detaiModel.pre_sale_time = 0;
                            detaiModel.is_group_pre_sale = 0;
                            detaiModel.is_sku_pre_sale = 1;
                            detaiModel.sku_list = item.sku_list;

                            if (detaiModel.shipment_limit_second > 172800)
                                detaiModel.shipment_limit_second = 172800;
                            break;
                        default:
                            break;
                    }
                    //数据正常请求接口
                    Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "type", "pdd.goods.information.update" },
                    { "client_id", apiHelp.client_id },
                    { "access_token", item.Malls.mall_token },
                    { "timestamp", ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000/1000).ToString() },
                    { "data_type", "JSON" },
                    //{ "goods_id", item.goods_id.ToString() },

                };

                    parameters.Add("carousel_gallery", JsonConvert.SerializeObject(detaiModel.carousel_gallery_list).ToString());
                    parameters.Add("detail_gallery", JsonConvert.SerializeObject(detaiModel.detail_gallery_list).ToString());

                    foreach (PropertyInfo property in detaiModel.GetType().GetProperties())
                    {
                        object value = property.GetValue(detaiModel);

                        List<string> Names = new List<string>() { "oversea_goods", "goods_travel_attr", "goods_trade_attr", "goods_properties", "elec_goods_attributes", "carousel_video", "carousel_gallery_list", "detail_gallery_list" };
                        if (Names.Contains(property.Name))
                            continue;

                        if (property.Name == "sku_list")
                        {
                            var settings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            };

                            foreach (var sku in detaiModel.sku_list)
                            {
                                List<long> specIds = new List<long>();
                                foreach (var spe in sku.spec)
                                    specIds.Add(spe.spec_id);
                                sku.spec_id_list = JsonConvert.SerializeObject(specIds.ToArray()).ToString();
                            }

                            value = JsonConvert.SerializeObject(detaiModel.sku_list, settings);
                        }

                        if (value != null && value.ToString() != "") // 检查值是否为null  
                        {
                            //某些参数类型变化
                            List<string> TypeChanged = new List<string>() { "is_customs", "is_folt", "is_pre_sale", "is_refundable", "second_hand" };
                            if (TypeChanged.Contains(property.Name))
                                parameters.Add(property.Name, value.ToString() == "1" ? "true" : "false");
                            else
                                parameters.Add(property.Name, value.ToString());
                        }
                    }
                    backMsg = apiHelp.SendPddApi(parameters);
                    results.Add(backMsg);
                }
                int ErrorSum = 0;
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].Code != 0)
                    {
                        ErrorSum++;
                        back.Mess += $"\n【{BodyList[i].goods_id}】:{results[i].Mess}";
                        continue;
                    }
                    //获取修改数据的id
                }
                back.Code = 0;
                back.Mess = $"修改完成：商品总数{results.Count},成功{results.Count - ErrorSum}个,失败{ErrorSum}个！" + back.Mess;
            }
            catch (Exception ex)
            {
                back.Code = 100;
                back.Mess= ex.Message;
            }
            return back;
        }

        [HttpPost("data/update")]
        public BackMsg update([FromBody] List<GoodListResponse> goods)
        {
            BackMsg result = new BackMsg();
            foreach (var good in goods)
            {
                string sql = "";
                if (!string.IsNullOrEmpty(good.goods_name))//修改标题
                {
                    sql = $"update good_list set goods_name='{good.goods_name}' where goods_id={good.goods_id} ";
                }
                else if (!string.IsNullOrEmpty(good.goods_quantity.ToString()))
                {
                    sql = $"update good_list set goods_quantity={good.goods_quantity},sku_list='{JsonConvert.SerializeObject(good.sku_list) }' where goods_id={good.goods_id} ";
                }
                SQLHelper.ExecuteCommand1(sql);
            }
            return result;
        }

        [HttpPost("qrcode/get")]
        public BackMsg getQrcode(long goods_id)
        {
            BackMsg result = new BackMsg();
            result.Code = 0;
            try
            {
                string url = "https://mobile.yangkeduo.com/goods1.html?goods_id=" + goods_id;
                // 使用MemoryStream来保存Bitmap对象  
                using (MemoryStream memory = new MemoryStream())
                {
                    Bitmap bitmap = QrCodeHelper.GeneratorQrImage(url);
                    // 将Bitmap对象保存到MemoryStream中  
                    bitmap.Save(memory, System.DrawingCore.Imaging.ImageFormat.Png); // 或者使用ImageFormat.Jpeg等  
                    memory.Position = 0; // 重置MemoryStream的位置  
                    // 读取MemoryStream中的数据并转换为Base64字符串  
                    byte[] byteImage = memory.ToArray();
                    string base64String = Convert.ToBase64String(byteImage);

                    result.Mess = base64String;
                }

            }
            catch (Exception ex)
            {
                result.Code = 100;
                result.Mess=ex.Message;
            }
            return  result;
        }

        [HttpPost("data/clear")]
        public BackMsg clear([FromBody] GoodRequest goods)
        {
            //清空缓存数据
            BackMsg result = new BackMsg();
            List<string> ids = new List<string>();
            foreach (var m in goods.mallinfos)
            {
                ids.Add(m.mall_id.ToString());
            }
            if (ids.Count > 0)
            {
                string sql = " delete good_list where mall_id in (" + string.Join(",", ids) + ")";
                result.Mess = SQLHelper.ExecuteCommand1(sql);

            }
            return result;
        }

    }

    public class GoodRequest
    {
        public string good_ids { get; set; } = "";
        public string good_name { get; set; } = "";

        public bool isOffend {  get; set; }=false;
        public string good_quantity { get; set; } = ""; 

        public int isOnsale {  get; set; } = -1;

        public int page { get; set; } = 0;
        public int page_size { get; set;} = 10;

        public List<Mallinfo> mallinfos { get; set; }=new List<Mallinfo>();
    }
}
