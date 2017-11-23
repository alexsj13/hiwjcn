using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Lib.core;
using Lib.helper;
using Lib.extension;
using System.Threading;
using Hiwjcn.Core.Model.Sys;
using System.Diagnostics;
using Lib.ioc;
using Lib.data;
using Lib.net;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 异步记录请求记录
    /// </summary>
    public class SendMailActor : ReceiveActor
    {
        public SendMailActor()
        {
            this.Receive<EmailModel>(x =>
            {
                try
                {
                    if (!EmailSender.SendMail(x))
                    {
                        $"发送邮件失败，邮件内容：{x?.ToJson()}".AddBusinessInfoLog();
                    }
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }
}
