using Lib.extension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mq.rabbitmq
{
    /// <summary>
    /// rabitmq，配置
    /// </summary>
    public class RabbitMqSection : ConfigurationSection
    {
        public static RabbitMqSection FromSection(string name)
        {
            return (RabbitMqSection)ConfigurationManager.GetSection(name);
        }

        [ConfigurationProperty(nameof(HostName), IsRequired = true)]
        public string HostName
        {
            get { return this[nameof(HostName)].ToString(); }
        }

        [ConfigurationProperty(nameof(UserName), IsRequired = true)]
        public string UserName
        {
            get { return this[nameof(UserName)].ToString(); }
        }

        [ConfigurationProperty(nameof(Password), IsRequired = false)]
        public string Password
        {
            get { return this[nameof(Password)].ToString(); }
        }

        [ConfigurationProperty(nameof(VirtualHost), IsRequired = false)]
        public string VirtualHost
        {
            get { return this[nameof(VirtualHost)].ToString(); }
        }

        /// <summary>
        /// 默认3000ms
        /// </summary>
        [ConfigurationProperty(nameof(ContinuationTimeout), IsRequired = false)]
        public int ContinuationTimeout
        {
            get => (this[nameof(ContinuationTimeout)]?.ToString() ?? "3000").ToInt();
        }

        /// <summary>
        /// 默认2000ms
        /// </summary>
        [ConfigurationProperty(nameof(SocketTimeout), IsRequired = false)]
        public int SocketTimeout
        {
            get => (this[nameof(SocketTimeout)]?.ToString() ?? "2000").ToInt();
        }
    }
}
