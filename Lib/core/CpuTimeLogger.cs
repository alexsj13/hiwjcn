using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Lib.helper;
using Lib.extension;

namespace Lib.core
{
    /// <summary>
    /// 运行计时
    /// </summary>
    public class CpuTimeLogger : IDisposable
    {
        private readonly Stopwatch timer = new Stopwatch();

        public Action<long, string> OnStop { get; set; }

        public string LoggerName { get; set; }

        public CpuTimeLogger(Action<long, string> OnStop, string LoggerName = null)
        {
            this.OnStop = OnStop ?? ((ms, name) =>
            {
                $"{name}|耗时：{ms}毫秒".AddInfoLog(name);
            });
            this.LoggerName = LoggerName ?? Com.GetUUID();

            this.timer.Start();
        }

        public void Dispose()
        {
            this.timer.Stop();
            this.OnStop?.Invoke(this.timer.ElapsedMilliseconds, this.LoggerName);
        }
    }
}
