using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Lib.core;
using Lib.helper;
using Lib.extension;
using Lib.ioc;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Lib.mvc;

namespace Lib.core
{
    /// <summary>
    /// 设置
    /// </summary>
    public interface ISettings
    {
        List<string> AllowDomains { get; }

        string ComDbConStr { get; }

        string MySqlConnectionString { get; }

        string MsSqlConnectionString { get; }

        string RedisConnectionString { get; }

        string WebToken { get; }

        string SSOLoginUrl { get; }

        string SSOLogoutUrl { get; }

        string CheckLoginInfoUrl { get; }

        string DefaultRedirectUrl { get; }

        string CookieDomain { get; }

        string SmptServer { get; }

        string SmptLoginName { get; }

        string SmptPassWord { get; }

        string SmptSenderName { get; }

        string SmptEmailAddress { get; }

        string FeedBackEmail { get; }

        string QiniuAccessKey { get; }

        string QiniuSecretKey { get; }

        string QiniuBucketName { get; }

        string QiniuBaseUrl { get; }
        /// <summary>
        /// 账户登录cookie保存分钟
        /// </summary>
        int CookieExpiresMinutes { get; }

        /// <summary>
        /// 缓存失效时间
        /// </summary>
        int CacheExpiresMinutes { get; }
        /// <summary>
        /// 是否是调试状态
        /// </summary>
        bool IsDebug { get; }
        /// <summary>
        /// 是否加载插件
        /// </summary>
        bool LoadPlugin { get; }
        /// <summary>
        /// 系统默认编码
        /// </summary>
        Encoding SystemEncoding { get; }
    }

    /// <summary>
    /// 加载当前上下文项目config文件中的配置
    /// </summary>
    public class BasicConfigProvider : ISettings
    {
        /// <summary>
        /// 从web.config中加载配置
        /// </summary>
        public BasicConfigProvider()
        {
            this.AllowDomains = ConfigurationManager.AppSettings[nameof(AllowDomains)]?.Split(',')?.ToList();
            if (this.AllowDomains == null) { this.AllowDomains = new List<string>(); }
            //string
            this.ComDbConStr = ConfigurationManager.ConnectionStrings[nameof(ComDbConStr)]?.ToString();
            this.MySqlConnectionString = ConfigurationManager.ConnectionStrings[nameof(MySqlConnectionString)]?.ToString();
            this.RedisConnectionString = ConfigurationManager.ConnectionStrings[nameof(RedisConnectionString)]?.ToString();
            this.MsSqlConnectionString = ConfigurationManager.ConnectionStrings[nameof(MsSqlConnectionString)]?.ToString();
            //单点登录配置
            this.SSOLoginUrl = ConfigurationManager.AppSettings[nameof(SSOLoginUrl)];
            this.SSOLogoutUrl = ConfigurationManager.AppSettings[nameof(SSOLogoutUrl)];
            this.CheckLoginInfoUrl = ConfigurationManager.AppSettings[nameof(CheckLoginInfoUrl)];
            this.WebToken = ConfigurationManager.AppSettings[nameof(WebToken)];
            this.DefaultRedirectUrl = ConfigurationManager.AppSettings[nameof(DefaultRedirectUrl)];

            this.CookieDomain = ConfigurationManager.AppSettings[nameof(CookieDomain)];

            //邮件发送配置
            this.SmptServer = ConfigurationManager.AppSettings[nameof(SmptServer)];
            this.SmptLoginName = ConfigurationManager.AppSettings[nameof(SmptLoginName)];
            this.SmptPassWord = ConfigurationManager.AppSettings[nameof(SmptPassWord)];
            this.SmptSenderName = ConfigurationManager.AppSettings[nameof(SmptSenderName)];
            this.SmptEmailAddress = ConfigurationManager.AppSettings[nameof(SmptEmailAddress)];

            this.FeedBackEmail = ConfigurationManager.AppSettings[FeedBackEmail];

            this.QiniuAccessKey = ConfigurationManager.AppSettings[nameof(QiniuAccessKey)];
            this.QiniuSecretKey = ConfigurationManager.AppSettings[nameof(QiniuSecretKey)];
            this.QiniuBucketName = ConfigurationManager.AppSettings[nameof(QiniuBucketName)];
            this.QiniuBaseUrl = ConfigurationManager.AppSettings[QiniuBaseUrl];

            //integer
            this.CookieExpiresMinutes = ConvertHelper.GetInt(ConfigurationManager.AppSettings[nameof(CookieExpiresMinutes)], 525600);
            this.CacheExpiresMinutes = ConvertHelper.GetInt(ConfigurationManager.AppSettings[nameof(CacheExpiresMinutes)], 5);

            this.LoadPlugin = (ConfigurationManager.AppSettings[nameof(LoadPlugin)] ?? "").ToBool();
        }

        public virtual List<string> AllowDomains { get; private set; }

        public virtual string ComDbConStr { get; private set; }

        public virtual string MySqlConnectionString { get; private set; }

        public virtual string MsSqlConnectionString { get; private set; }

        public virtual string RedisConnectionString { get; private set; }

        public virtual string WebToken { get; private set; }

        public virtual string SSOLoginUrl { get; private set; }

        public virtual string SSOLogoutUrl { get; private set; }

        public virtual string CheckLoginInfoUrl { get; private set; }

        public virtual string DefaultRedirectUrl { get; private set; }

        public virtual string CookieDomain { get; private set; }

        public virtual string SmptServer { get; private set; }

        public virtual string SmptLoginName { get; private set; }

        public virtual string SmptPassWord { get; private set; }

        public virtual string SmptSenderName { get; private set; }

        public virtual string SmptEmailAddress { get; private set; }

        public virtual string FeedBackEmail { get; private set; }

        public virtual string QiniuAccessKey { get; private set; }

        public virtual string QiniuSecretKey { get; private set; }

        public virtual string QiniuBucketName { get; private set; }

        public virtual string QiniuBaseUrl { get; private set; }
        /// <summary>
        /// 账户登录cookie保存分钟
        /// </summary>
        public virtual int CookieExpiresMinutes { get; private set; }

        /// <summary>
        /// 缓存失效时间
        /// </summary>
        public virtual int CacheExpiresMinutes { get; private set; }
        /// <summary>
        /// 是否是调试状态
        /// </summary>
        public virtual bool IsDebug { get; private set; }
        /// <summary>
        /// 是否加载插件
        /// </summary>
        public virtual bool LoadPlugin { get; private set; }
        /// <summary>
        /// 系统默认编码
        /// </summary>
        public virtual Encoding SystemEncoding { get { return Encoding.UTF8; } }
    }

    /// <summary>
    /// 基于json的实现
    /// </summary>
    public class JsonConfigProvider : ISettings
    {
        private readonly JToken _config;
        public JsonConfigProvider()
        {
            var path = ServerHelper.GetMapPath("~/App_Data");
            this._config = JToken.Parse(File.ReadAllText(Path.Combine(path, "config.json")));
            if (this._config == null) { throw new ArgumentException(nameof(this._config)); }
        }

        public string GetConnectionString(string name)
        {
            return (string)this._config.SelectToken($"$.connectionStrings[?(@.name=='{name}')]");
        }

        public string GetAppSetting(string name)
        {
            return (string)this._config.SelectToken($"$.appSettings[?(@.name=='{name}')]");
        }


        public List<string> AllowDomains
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int CacheExpiresMinutes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CheckLoginInfoUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ComDbConStr
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CookieDomain
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int CookieExpiresMinutes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DefaultRedirectUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FeedBackEmail
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDebug
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool LoadPlugin
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string MsSqlConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string MySqlConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuAccessKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuBaseUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuBucketName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuSecretKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string RedisConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptEmailAddress
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptLoginName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptPassWord
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptSenderName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptServer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SSOLoginUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SSOLogoutUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Encoding SystemEncoding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string WebToken
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// 基于xml的实现
    /// </summary>
    public class XmlConfigProvider : ISettings
    {
        public List<string> AllowDomains
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int CacheExpiresMinutes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CheckLoginInfoUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ComDbConStr
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string CookieDomain
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int CookieExpiresMinutes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DefaultRedirectUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FeedBackEmail
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDebug
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool LoadPlugin
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string MsSqlConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string MySqlConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuAccessKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuBaseUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuBucketName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string QiniuSecretKey
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string RedisConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptEmailAddress
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptLoginName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptPassWord
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptSenderName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SmptServer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SSOLoginUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SSOLogoutUrl
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Encoding SystemEncoding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string WebToken
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// 加载当前上下文项目config文件中的配置
    /// </summary>
    public static class ConfigHelper
    {
        private static ISettings ins = null;

        private static readonly object locker = new object();

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static ISettings Instance
        {
            get
            {
                if (ins == null)
                {
                    lock (locker)
                    {
                        if (ins == null)
                        {
                            ins = new BasicConfigProvider();
                        }
                    }
                }
                return ins;
            }
        }
    }
}
