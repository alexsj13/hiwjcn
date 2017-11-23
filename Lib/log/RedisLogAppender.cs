using Lib.data;
using Lib.data.redis;
using log4net.Appender;
using log4net.Core;
using System;

namespace Lib.log
{
    public class LogLine
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1);

        public LogLine(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null) { throw new Exception("loggingEvent不能为null"); }
            HostName = loggingEvent.LookupProperty(LoggingEvent.HostNameProperty).ToString();
            Identity = loggingEvent.Identity;
            UserName = loggingEvent.UserName;
            Domain = loggingEvent.Domain;
            TimeStamp = (long)(loggingEvent.TimeStamp.ToUniversalTime() - EpochStart).TotalMilliseconds;
            Level = loggingEvent.Level.DisplayName;
            LoggerName = loggingEvent.LoggerName;
            Thread = loggingEvent.ThreadName;
            Message = loggingEvent.RenderedMessage;
            Throwable = loggingEvent.GetExceptionString();
            //location
            Class = loggingEvent.LocationInformation?.ClassName;
            Method = loggingEvent.LocationInformation?.MethodName;
            File = loggingEvent.LocationInformation?.FileName;
            Line = loggingEvent.LocationInformation?.LineNumber;
        }

        public string HostName { get; set; }
        public string Identity { get; set; }
        public string UserName { get; set; }
        public string Domain { get; set; }
        public string LoggerName { get; set; }
        public long TimeStamp { get; set; }
        public string Level { get; set; }
        public string Thread { get; set; }
        public string Message { get; set; }
        public string Throwable { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public string File { get; set; }
        public string Line { get; set; }
    }
    /// <summary>
    /// 使用redis存储日志
    /// https://github.com/lokki/RedisAppender
    /// </summary>
    public class RedisLogAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var logline = new LogLine(loggingEvent);
            using (var client = new RedisHelper())
            {
                client.ListLeftPush(nameof(RedisLogAppender), logline);
            }
        }
    }
}