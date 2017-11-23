using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;

namespace Lib.core
{
    public class StepLog : IDisposable
    {
        class Step
        {
            public Step(string content)
            {
                this.Content = content;
                this.Time = DateTime.Now;
            }

            public string Content { get; set; }
            public DateTime Time { get; set; }
        }

        private readonly List<Step> list = new List<Step>();

        private readonly string LogName;

        public StepLog(string name)
        {
            this.LogName = name ?? throw new ArgumentNullException(nameof(name));
            this.AddStep($"创建名为{this.LogName}的步骤日志");
        }

        public void AddStep(string content)
        {
            this.list.Add(new Step(content));
        }

        public void AddException(Exception e)
        {
            this.AddStep(e.GetInnerExceptionAsJson());
        }

        public void Dispose()
        {
            list.Select(x => x.ToJson()).AsSteps().AddBusinessInfoLog();

            list.Clear();
        }
    }
}
