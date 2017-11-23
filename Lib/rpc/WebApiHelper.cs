using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Polly;

namespace Lib.rpc
{
    public class ServerSetting
    {
        public string Server { get; set; }

        public byte Weight { get; set; }

        private long CallTimes { get; set; }

        /// <summary>
        /// 记录请求，重新计算权重
        /// </summary>
        public void LogRequest() { }
    }

    /// <summary>
    /// api调用，自动降权升权，按照权重选择服务器
    /// </summary>
    public class WebApiHelper
    {
        public WebApiHelper()
        {
            var server_set = new List<ServerSetting>();
            var server_balance = server_set.ToDictionary(x => x, x => (int)x.Weight);

            var ran = new Random((int)DateTime.Now.Ticks);
            var server = ran.ChoiceByWeight(server_balance).Server;
        }

        private void Send(ServerSetting server)
        {
            Action<long, string> logger = (time, name) =>
            {
                server.LogRequest();
            };
            using (var timer = new CpuTimeLogger(logger, "xxx"))
            {
                //send the requst
            }
        }

        public T Post<T>(string path, object param)
        {
            throw new NotImplementedException();
        }

    }
}
