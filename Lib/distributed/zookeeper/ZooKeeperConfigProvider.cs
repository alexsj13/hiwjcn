using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.distributed.zookeeper
{
    /// <summary>
    /// 从ZooKeeper读取配置
    /// </summary>
    public class ZooKeeperConfigProvider : ISettings
    {
        public ZooKeeperConfigProvider(string configurationName, string path)
        {
            //load data from zk
            //parse json and assign data
        }

        public List<string> AllowDomains
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

        public string MySqlConnectionString
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

        public string RedisConnectionString
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

        public string CheckLoginInfoUrl
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

        public string CookieDomain
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

        public string SmptEmailAddress
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

        public string QiniuAccessKey
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

        public string QiniuBucketName
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

        public int CookieExpiresMinutes
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

        public Encoding SystemEncoding
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
