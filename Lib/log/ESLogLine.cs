using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using log4net.Core;
using Lib.helper;
using Lib.extension;
using Lib.data.elasticsearch;

namespace Lib.log
{
    [ElasticsearchType(IdProperty = nameof(UID), Name = nameof(ESLogLine))]
    public class ESLogLine : IElasticSearchIndex
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1);

        [Obsolete("不要调用")]
        public ESLogLine() { }

        public ESLogLine(LoggingEvent loggingEvent)
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

            this.UID = Com.GetUUID();
            this.CreateTime = DateTime.Now;
            this.UpdateTime = this.CreateTime;
        }

        [String(Name = nameof(UID), Index = FieldIndexOption.NotAnalyzed)]
        public string UID { get; set; }

        [String(Name = nameof(HostName), Index = FieldIndexOption.NotAnalyzed)]
        public string HostName { get; set; }

        [String(Name = nameof(Identity), Index = FieldIndexOption.NotAnalyzed)]
        public string Identity { get; set; }

        [String(Name = nameof(UserName), Index = FieldIndexOption.NotAnalyzed)]
        public string UserName { get; set; }

        [String(Name = nameof(Domain), Index = FieldIndexOption.NotAnalyzed)]
        public string Domain { get; set; }

        [String(Name = nameof(LoggerName), Index = FieldIndexOption.NotAnalyzed)]
        public string LoggerName { get; set; }

        [Number(Name = nameof(TimeStamp), Index = NonStringIndexOption.NotAnalyzed)]
        public long TimeStamp { get; set; }

        [String(Name = nameof(Level), Index = FieldIndexOption.NotAnalyzed)]
        public string Level { get; set; }

        [String(Name = nameof(Thread), Index = FieldIndexOption.NotAnalyzed)]
        public string Thread { get; set; }

        [String(Name = nameof(Message), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public string Message { get; set; }

        [String(Name = nameof(Throwable), Index = FieldIndexOption.NotAnalyzed)]
        public string Throwable { get; set; }

        [String(Name = nameof(Class), Index = FieldIndexOption.NotAnalyzed)]
        public string Class { get; set; }

        [String(Name = nameof(Method), Index = FieldIndexOption.NotAnalyzed)]
        public string Method { get; set; }

        [String(Name = nameof(File), Index = FieldIndexOption.NotAnalyzed)]
        public string File { get; set; }

        [String(Name = nameof(Line), Index = FieldIndexOption.NotAnalyzed)]
        public string Line { get; set; }

        [Date(Name = nameof(CreateTime), Index = NonStringIndexOption.NotAnalyzed)]
        public DateTime CreateTime { get; set; }

        [Date(Name = nameof(UpdateTime), Index = NonStringIndexOption.NotAnalyzed)]
        public DateTime UpdateTime { get; set; }
    }
}
