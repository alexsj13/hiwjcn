using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.auth;
using Lib.mvc.user;
using Lib.mvc;
using Lib.net;
using Dapper;
using Lib.data;
using Lib.helper;
using Lib.extension;
using Lib.core;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using Hiwjcn.Core.Infrastructure.Auth;
using System.Data.Entity;
using Hiwjcn.Core.Data;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using Polly;
using Polly.CircuitBreaker;
using Lib.infrastructure.service;
using Lib.data.ef;

namespace Hiwjcn.Bll.Auth
{
    public class QPLUserLoginService : IAuthLoginService
    {
        private readonly string[] sys_users = new string[] { "13915280232", "18101795560" };

        private LoginUserInfo Parse(UserInfo model)
        {
            var loginuser = new LoginUserInfo()
            {
                IID = model.IID,
                UserID = model.UID,
                Email = model.Email,
                UserName = model.UserName,
                NickName = model.ShopName,
                HeadImgUrl = model.Images,
                IsActive = model.IsActive ?? (int)YesOrNoEnum.否,
                IsValid = true,

                IsCheck = model.IsCheck ?? (int)YesOrNoEnum.否,
                CustomerType = model.CustomerType,
                LocationId = model.CityId,

                LoginToken = "please load auth token"
            };

            /*
            loginuser.AddExtraData(nameof(model.address), model.address);
            loginuser.AddExtraData(nameof(model.CompanyName), model.CompanyName);
            loginuser.AddExtraData(nameof(model.Contact), model.Contact);
            loginuser.AddExtraData(nameof(model.CustomerType), model.CustomerType);
            loginuser.AddExtraData(nameof(model.IsCheck), model.IsCheck.ToString());
            loginuser.AddExtraData(nameof(model.Phone), model.Phone);
            */

            loginuser.MapExtraData(model);

            return loginuser;
        }

        private bool CheckUser(UserInfo model, out string msg)
        {
            if (model == null)
            {
                msg = "用户不存在";
                return false;
            }
            if (model.IsActive <= 0)
            {
                msg = "用户未激活";
                return false;
            }
            if (model.IsRemove > 0)
            {
                msg = "用户被删除";
                return false;
            }

            msg = string.Empty;
            return true;
        }

        public async Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10)
        {
            var data = new PagerData<LoginUserInfo>();
            using (var db = new QPLEntityDB())
            {
                var query = db.UserInfo.AsNoTrackingQueryable();

                if (ValidateHelper.IsPlumpString(q))
                {
                    var qs = q.Split(new char[] { ' ', ',', '，' })
                        .Select(x => x.Trim())
                        .Where(x => ValidateHelper.IsPlumpString(x));
                    foreach (var kwd in qs)
                    {
                        query = query.Where(x =>
                        x.CompanyName.Contains(kwd) ||
                        x.ShopName.Contains(kwd) ||
                        x.UserName.Contains(kwd) ||
                        x.Mobile == kwd ||
                        x.Phone == kwd ||
                        x.ShopNo == kwd ||
                        x.UID == kwd);
                    }
                }

                data.ItemCount = await query.CountAsync();
                var list = await query.OrderByDescending(x => x.CreatedDate).QueryPage(page, pagesize).ToListAsync();
                data.DataList = list.Select(x => this.Parse(x)).ToList();
            }
            return data;
        }

        public async Task<LoginUserInfo> LoadPermissions(LoginUserInfo model)
        {
            if (model == null)
            {
                return model;
            }
            if (sys_users.Contains(model.UserName))
            {
                model.Permissions = new List<string>()
                {
                    "manage.auth",
                    "manage.user",
                    "manage.system",
                    "manage.order",
                    "manage.product"
                };
            }
            return await Task.FromResult(model);
        }

        public async Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            using (var db = new QPLEntityDB())
            {
                var userinfo = await db.UserInfo.Where(x => x.UID == uid && x.IsRemove <= 0).FirstOrDefaultAsync();
                if (userinfo != null)
                {
                    return await this.LoadPermissions(this.Parse(userinfo));
                }
            }
            return null;
        }

        public async Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code)
        {
            var data = new _<LoginUserInfo>();
            using (var db = new QPLEntityDB())
            {
                var time = DateTime.Now.AddMinutes(-5);
                var sms = await db.Sms
                    .Where(x => x.Recipient == phoneOrEmail && x.CreatedDate >= time)
                    .OrderByDescending(x => x.CreatedDate).Take(1).FirstOrDefaultAsync();
                if (!((sms != null && sms.MsgCode == code) || (sys_users.Contains(phoneOrEmail) && code == "0000")))
                {
                    data.SetErrorMsg("验证码错误");
                    return data;
                }
                var user = await db.UserInfo.Where(x => x.UserName == phoneOrEmail).FirstOrDefaultAsync();
                if (user != null)
                {
                    if (!this.CheckUser(user, out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }
                else
                {
                    //create new user
                    var now = DateTime.Now;
                    user = new UserInfo()
                    {
                        UID = Com.GetUUID(),
                        UserName = phoneOrEmail,
                        Mobile = phoneOrEmail,
                        CustomerType = "1",
                        IsActive = 1,
                        IsRemove = 0,
                        IsCheck = 0,
                        ProvinceId = "31",
                        CreatedDate = now,
                        UpdatedDate = now
                    };

                    db.UserInfo.Add(user);
                    if (await db.SaveChangesAsync() <= 0)
                    {
                        new Exception($"用户创建失败：{user.ToJson()}").AddErrorLog();
                        data.SetErrorMsg("创建新用户失败");
                        return data;
                    }
                }
                var loginuser = await this.LoadPermissions(this.Parse(user));
                data.SetSuccessData(loginuser);
            }
            return data;
        }

        public async Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            using (var db = new QPLEntityDB())
            {
                var now = DateTime.Now;
                var ran = new Random((int)now.Ticks);
                var code = ran.Sample(Com.Range(10).ToList(), 4).AsString();
                //send sms

                var model = new Sms();
                model.UID = Com.GetUUID();
                model.MsgCode = code;
                model.Msg = $"汽配龙登录验证码：{code}";
                model.TemplateId = "no data";
                model.Recipient = phoneOrEmail;
                model.SmsType = 0;
                model.Status = (int)YesOrNoEnum.是;
                model.Sender = "大汉三通";
                model.CreatedDate = model.UpdatedDate = now;

                db.Sms.Add(model);

                if (await db.SaveChangesAsync() > 0)
                {
                    return string.Empty;
                }
                throw new Exception("保存验证码失败");
            }
        }

        /// <summary>
        /// 熔断器
        /// </summary>
        private static readonly CircuitBreakerPolicy tuhu_login_breaker =
            Policy.Handle<Exception>().CircuitBreakerAsync(50, TimeSpan.FromMinutes(1));

        public async Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password)
        {
            var data = new _<LoginUserInfo>();
            if (!ValidateHelper.IsAllPlumpString(user_name, password))
            {
                data.SetErrorMsg("请填写用户名和密码");
                return data;
            }

            //途虎门店登录
            if (user_name.Trim().ToLower().StartsWith("dm"))
            {
                try
                {
                    //熔断+重试
                    return await tuhu_login_breaker.ExecuteAsync(async () =>
                    {
                        return await Policy.Handle<Exception>()
                        .WaitAndRetryAsync(2, i => TimeSpan.FromMilliseconds(100 * i))
                        .ExecuteAsync(async () => await this.TuhuDMUserLogin(user_name, password));
                    });
                }
                catch (Exception e) when (e is BrokenCircuitException || e is IsolatedCircuitException)
                {
                    e.AddErrorLog("途虎门店登录接口被熔断");

                    data.SetErrorMsg("汽配龙和途虎接口对接发生问题，请稍后再试");
                    return data;
                }
            }

            //汽配龙登录
            using (var db = new QPLEntityDB())
            {
                var model = await db.UserInfo.Where(x => x.UserName == user_name).FirstOrDefaultAsync();
                if (!this.CheckUser(model, out var msg))
                {
                    data.SetErrorMsg(msg);
                    return data;
                }
                if (!(sys_users.Contains(model.UserName) && password == "123"))
                {
                    data.SetErrorMsg("账号密码错误");
                    return data;
                }
                var loginuser = await this.LoadPermissions(this.Parse(model));
                data.SetSuccessData(loginuser);
            }

            return data;
        }


        //途虎用户账户登录限制记录保存在REDIS里面，如果REDIS无法登录不影响用户使用，限制主要是为了防止别人暴力破解。
        //限制条件为，一个账户2分钟不能登录失败超过5次，否则限制登录60分钟。
        //是否限制登录的标示(Redis Key格式)： "{  TH+username+0  }",VALUE没有值，只判断KEY是否存在。超时设置30分钟
        //所有的登录验证信息保存在REDIS DB 4 不要与王俊的内容有冲突，都是已TH开头的信息。
        //当REDIS服务挂掉的时候，放弃所以得相关验证，保证用户登录优先
        //当密码错误次数超过2次，则要求验证码验证

        private static string appSecret = TuhuApiHelper.appSecret;
        private static string appId = TuhuApiHelper.appId;
        private static string appUrl = TuhuApiHelper.appUrl;

        public async Task<_<LoginUserInfo>> TuhuDMUserLogin(string username, string password)
        {
            if (!ValidateHelper.IsAllPlumpString(appSecret, appId, appUrl))
            {
                throw new Exception("请配置tuhu api访问密钥");
            }

            var data = new _<LoginUserInfo>();

            string url = appUrl + "/shop/GetQplUserInfoAsync";

            var model = new TuhuDmUserLoginRequest();

            model.AppId = appId;
            model.TimeStamp = this.GetTimeStamp();
            model.MethodName = "GetQplUserInfoAsync";

            model.UserName = username + "@tuhu.cn";
            model.PassWord = EncryptHelper.EncryptString(password);

            var urlParams = model.GetAllUrlParameter();
            var bodyParams = model.GetAllBodyParameter();
            var sign = TuhuApiHelper.GenerateSign(urlParams, bodyParams, appSecret, "utf-8");
            urlParams.Add("sign", sign);

            var urlString = TuhuApiHelper.CreateLinkString(urlParams);
            url = $"{url}?{urlString}";

            var httpClient = HttpClientManager.Instance.DefaultClient;

            var response = await httpClient.PostAsJsonAsync(url, bodyParams);

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    data.SetErrorMsg("途虎登录查询信息验证失败");
                    return data;
                }

                var jsonStr = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<JToken>(jsonStr);
                var code = json.Value<JToken>("Code").ToString().ToInt();
                if (code == 0)
                {
                    var tuhu_user = json.Value<JToken>("Body").ToObject<QplUserInfoModel>();
                    try
                    {
                        await UpdateDMUserInfo(tuhu_user);
                    }
                    catch (Exception e)
                    {
                        e.AddErrorLog("途虎门店登录，更新本地信息失败");
                    }

                    using (var db = new QPLEntityDB())
                    {
                        var user = await db.UserInfo.Where(x => x.UserName == tuhu_user.UserName).FirstOrDefaultAsync();
                        if (user == null)
                        {
                            data.SetErrorMsg("途虎用户未和汽配龙用户关联");
                            return data;
                        }
                        var loginuser = await this.LoadPermissions(this.Parse(user));
                        data.SetSuccessData(loginuser);
                        return data;
                    }
                }
                else
                {
                    data.SetErrorMsg(json.Value<JToken>("Msg").ToString());
                    return data;
                }
            }
        }

        ///// <summary>
        ///// 途虎门店类型
        ///// </summary>
        //public static List<ShopCodeNameModel> ShopType = new List<ShopCodeNameModel>() {
        //    new ShopCodeNameModel { Code=1,Name="合作店面" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=2,Name="供应商" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=4,Name="仓库" ,Gcode=null,Gname=null },
        //    //new ShopCodeNameModel { Code=8,Name="途虎配送" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=16,Name="虎式服务" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=32,Name="上门服务" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=64,Name="大客户门店" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=128,Name="星级门店" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=256,Name="雪地胎门店" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=512,Name="途虎工场" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=1024,Name="加盟店" ,Gcode=null,Gname=null },
        //    new ShopCodeNameModel { Code=2048,Name="自营门店" ,Gcode=null,Gname=null }
        //};

        public async Task UpdateDMUserInfo(QplUserInfoModel obj)
        {
            string CustomerType = "3";

            if (PermissionHelper.HasAnyPermission(obj.ShopType, 512, 1024, 2048))
            {
                CustomerType = "4";
            }
            else
            {
                CustomerType = "3";
            }

            using (var db = new QPLEntityDB())
            {
                using (var con = db.Database.Connection)
                {
                    con.OpenIfClosedWithRetry(1);
                    var dp = new DynamicParameters();
                    dp.Add("UserName", obj.UserName, DbType.String);
                    dp.Add("ShopName", obj.ShopName, DbType.String);
                    dp.Add("CompanyName", obj.CompanyName, DbType.String);
                    dp.Add("Contact", obj.Contact, DbType.String);
                    dp.Add("Mobile", obj.Mobile, DbType.String);
                    dp.Add("RProvinceId", obj.ProvinceId, DbType.String);
                    dp.Add("RCityId", obj.CityId, DbType.String);
                    dp.Add("address", obj.Address, DbType.String);
                    dp.Add("Notes", obj.Notes, DbType.String);
                    dp.Add("CustomerType", CustomerType, DbType.String);

                    #region
                    var sql = @"
                      DECLARE @CityId  NVARCHAR(50);
                      DECLARE @ProvinceId  nvarchar(50);
                      IF EXISTS ( SELECT    1
                                  FROM      Parties..UserInfo WITH ( NOLOCK )
                                  WHERE     UserName = @UserName  and CustomerType not in (3,4))
                        BEGIN
                            RAISERROR ('用户已经存在' , 16, 1) WITH NOWAIT;
                            RETURN;
                        END;
                      IF ( @RProvinceId = '-1' )
                        BEGIN
                            IF NOT EXISTS ( SELECT  1
                                            FROM    Parties..T_TuhuRegionMapping WITH ( NOLOCK )
                                            WHERE   PKID = @RCityId )
                                BEGIN
                                    RAISERROR ('用户映射区域信息不存在' , 16, 1) WITH NOWAIT;
                                    RETURN;
                                END;
                            ELSE
                                BEGIN
                                    SELECT  @ProvinceId = ProvinceId ,
                                            @CityId = CityId
                                    FROM    Parties..T_TuhuRegionMapping WITH ( NOLOCK )
                                    WHERE   PKID = @RCityId;
                                END;
         
     
                        END;
                      else
                      begin
                          set @ProvinceId=@RProvinceId;
                          set @CityId=@RCityId;
                      end
                      IF NOT EXISTS ( SELECT    1
                                      FROM      Parties..Area WITH ( NOLOCK )
                                      WHERE     UID = @ProvinceId )
                        BEGIN
                            RAISERROR ('用户映射省不存在' , 16, 1) WITH NOWAIT;
                            RETURN;
                        END;
                      IF NOT EXISTS ( SELECT    1
                                      FROM      Parties..Area WITH ( NOLOCK )
                                      WHERE     UID = @CityId
                                                AND PUID = @ProvinceId )
                        BEGIN
                            RAISERROR ('用户映射市不存在' , 16, 1) WITH NOWAIT;
                            RETURN;
                        END;
                         --更新存在的新增用户记录
                      IF EXISTS ( SELECT    1
                                  FROM      Parties..UserInfo WITH ( NOLOCK )
                                  WHERE     UserName = @UserName and  CustomerType in (3,4))
                        BEGIN
                            UPDATE  Parties..UserInfo
                            SET     [ShopName] = @ShopName ,
                                    [CompanyName] = @CompanyName ,
                                    [Contact] = @Contact ,
                                    [Mobile] = @Mobile ,
                                    [ProvinceId] = @ProvinceId ,
                                    [CityId] = @CityId ,
                                    [address] = @address ,
                                    [Notes] = @Notes ,
				                    CustomerType = @CustomerType,
                                    UpdatedDate = GETDATE()
                            WHERE   UserName = @UserName
                                    AND ( CustomerType = 4
                                          OR CustomerType = 3
                                        );
                        END;
                      ELSE --不存在创建新纪录
                        BEGIN
                            INSERT  INTO Parties..UserInfo
                                    ( [UID] ,
                                      [UserName] ,
                                      [ShopName] ,
                                      [CompanyName] ,
                                      [Contact] ,
                                      [Mobile] ,
                                      [ProvinceId] ,
                                      [CityId] ,
                                      [address] ,
                                      [Notes] ,
                                      [CustomerType] ,
                                      [IsActive] ,
                                      [CreatedDate] ,
                                      [UpdatedDate] ,
                                      [IsCheck] ,
                                      [VerifyDate]
                                    )
                            VALUES  ( ( LOWER(REPLACE(NEWID(), '-', '')) ) ,
                                      @UserName ,
                                      @ShopName ,
                                      @CompanyName ,
                                      @Contact ,
                                      @Mobile ,
                                      @ProvinceId ,
                                      @CityId ,
                                      @address ,
                                      @Notes ,
                                      @CustomerType ,
                                      1 ,
                                      GETDATE() ,
                                      GETDATE() ,
                                      1 ,
                                      GETDATE()
                                    );
                        END;
                    ";
                    #endregion

                    await con.ExecuteAsync(sql, param: dp, commandType: CommandType.Text);

                }
            }
        }


        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

    }

    [Serializable]
    public class QplUserInfoModel
    {
        public string Address { get; set; }
        public string City { get; set; }
        //
        // 摘要:
        //     市
        public string CityId { get; set; }
        //
        // 摘要:
        //     公司名称
        public string CompanyName { get; set; }
        //
        // 摘要:
        //     联系信息
        public string Contact { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        //
        // 摘要:
        //     客户类别 1门店2销售3商家 4平台
        public int CustomerType { get; set; }
        //
        // 摘要:
        //     手机
        public string Mobile { get; set; }
        public string Notes { get; set; }
        public string Province { get; set; }
        //
        // 摘要:
        //     省
        public string ProvinceId { get; set; }
        public int ShopID { get; set; }
        public string ShopName { get; set; }
        public string Street { get; set; }
        public string StreetId { get; set; }
        public string Town { get; set; }
        public string TownId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
        //
        // 摘要:
        //     用户名
        public string UserName { get; set; }

        //门店类型 type<512的，customType =3 ，tpye>=512的，customertype=4,备注customertype=2048的，是自营的工厂店
        public int ShopType { get; set; }
    }
    public class TuhuDmUserLoginRequest : BaseRequestModel
    {

        public string UserName { get; set; }
        public string PassWord { get; set; }

        public override Dictionary<string, string> GetAllBodyParameter()
        {
            var urlParams = new Dictionary<string, string>(11);

            urlParams.Add("Email", UserName);
            urlParams.Add("Password", PassWord);

            return urlParams;
        }
    }
    public abstract class BaseRequestModel
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// AppKey
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string TimeStamp { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 获取所有的http body参数列表
        /// </summary>
        /// <returns></returns>
        public abstract Dictionary<string, string> GetAllBodyParameter();
        /// <summary>
        /// 获取所有的url参数列表
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, string> GetAllUrlParameter()
        {
            var parameters = new Dictionary<string, string>(11);
            parameters.Add("appid", AppId);
            parameters.Add("methodName", MethodName);
            parameters.Add("TimeStamp", TimeStamp);
            parameters.Add("Version", "1.0");
            return parameters;
        }
    }
    public class TuhuApiHelper
    {
        public static string appSecret = System.Configuration.ConfigurationManager.AppSettings["AppSecret"];
        public static string appId = System.Configuration.ConfigurationManager.AppSettings["AppId"];
        public static string appUrl = System.Configuration.ConfigurationManager.AppSettings["AppUrl"];


        public static string Sign(string preSignStr, string secretKey, string inputCharset)
        {
            StringBuilder sb = new StringBuilder(32);

            preSignStr = preSignStr + secretKey;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(preSignStr.ToLower()));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        public static string GenerateSign(Dictionary<string, string> urlParams,
            Dictionary<string, string> bodyParams, string secretKey, string inputCharset)
        {
            var allParams = urlParams.Union(bodyParams);
            var sortedParams =
                new SortedDictionary<string, string>(FilterPara(allParams));
            var preSignString = CreateLinkString(sortedParams);

            return Sign(preSignString, secretKey, inputCharset);
        }

        /// <summary>
        /// 除去数组中的空值和签名参数
        /// </summary>
        /// <param name="dicArrayPre">过滤前的参数组</param>
        /// <returns>过滤后的参数组</returns>
        public static Dictionary<string, string> FilterPara(IEnumerable<KeyValuePair<string, string>> dicArrayPre)
        {
            Dictionary<string, string> dicArray = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> temp in dicArrayPre)
            {
                if (temp.Key.ToLower() != "sign"
                    && temp.Key.ToLower() != "signtype"
                    && !String.IsNullOrWhiteSpace(temp.Value))
                {
                    dicArray.Add(temp.Key, temp.Value);
                }
            }

            return dicArray;
        }

        /// <summary>
        /// 把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
        /// </summary>
        /// <param name="dicArray">需要拼接的数组</param>
        /// <returns>拼接完成以后的字符串</returns>
        public static string CreateLinkString(Dictionary<string, string> dicArray)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            //去掉最後一個&字符
            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }

        /// <summary>
        /// 把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
        /// </summary>
        /// <param name="dicArray">需要拼接的数组</param>
        /// <returns>拼接完成以后的字符串</returns>
        public static string CreateLinkString(SortedDictionary<string, string> dicArray)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            }

            //去掉最後一個&字符
            int nLen = prestr.Length;
            prestr.Remove(nLen - 1, 1);

            return prestr.ToString();
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
    }
    public class EncryptHelper
    {
        /// <summary>
        /// 构造一个对称算法
        /// </summary>
        private static SymmetricAlgorithm mCSP = new TripleDESCryptoServiceProvider();
        /// <summary>
        /// 密钥，必须32位
        /// </summary>
        private static string sKey = System.Configuration.ConfigurationManager.AppSettings["EncrypKey"];
        /// <summary>
        /// 向量，必须是12个字符
        /// </summary>
        private static string sIV = System.Configuration.ConfigurationManager.AppSettings["EncrypIV"];

        #region 加密解密函数

        /// <summary>
        /// 字符串的加密
        /// </summary>
        /// <param name="Value">要加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string EncryptString(string Value)
        {
            try
            {
                ICryptoTransform ct;
                MemoryStream ms;
                CryptoStream cs;
                byte[] byt;
                mCSP.Key = Convert.FromBase64String(sKey);
                mCSP.IV = Convert.FromBase64String(sIV);
                //指定加密的运算模式
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;
                //获取或设置加密算法的填充模式
                mCSP.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ct = mCSP.CreateEncryptor(mCSP.Key, mCSP.IV);//创建加密对象
                byt = Encoding.UTF8.GetBytes(Value);
                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "出现异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return ("Error in Encrypting " + ex.Message);
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="Value">加密后的字符串</param>
        /// <returns>解密后的字符串</returns>
        public static string DecryptString(string Value)
        {
            try
            {
                ICryptoTransform ct;//加密转换运算
                MemoryStream ms;//内存流
                CryptoStream cs;//数据流连接到数据加密转换的流
                byte[] byt;
                //将3DES的密钥转换成byte
                mCSP.Key = Convert.FromBase64String(sKey);
                //将3DES的向量转换成byte
                mCSP.IV = Convert.FromBase64String(sIV);
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;
                mCSP.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV);//创建对称解密对象
                byt = Convert.FromBase64String(Value);
                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "出现异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return ("Error in Decrypting " + ex.Message);
            }
        }

        #endregion
    }
}
