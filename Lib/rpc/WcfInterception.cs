using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;
using System.ServiceModel.Channels;
using System.Configuration;
using System.IO;
using System.Web;
using System.Reflection;
using Castle.DynamicProxy;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Lib.rpc
{
    [ServiceContract]
    public interface IWcfIntercept
    {
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Intercept(Message request);
    }

    [ServiceBehavior(UseSynchronizationContext = false, AddressFilterMode = AddressFilterMode.Any)]
    public class WcfInterceptionService : IWcfIntercept
    {
        public Message Intercept(Message request)
        {
            using (var channel = new ChannelFactory<IWcfIntercept>())
            {
                var interceptor = channel.CreateChannel();
                using (interceptor as IDisposable)
                {
                    var reqBuffer = request.CreateBufferedCopy(int.MaxValue);
                    var response = interceptor.Intercept(reqBuffer.CreateMessage());
                    var resBuffer = response.CreateBufferedCopy(int.MaxValue);
                    return resBuffer.CreateMessage();
                }
            }
        }
    }



    ///////////////////////////////////////////////////////////////////////////////////////////////////////



    /// <summary>  
    ///  消息拦截器  
    /// </summary>  
    public class MyMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
            var data = new
            {
                LogName = "Wcf调用结果",
                Request = correlationState?.ToString(),
                Response = reply.ToString(),
                Time = DateTime.Now
            }.ToJson();

            data.AddBusinessInfoLog();
        }

        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var rid = Com.GetUUID();

            // 插入验证信息
            var request_id = MessageHeader.CreateHeader("rid", "rid", rid);
            var hdUserName = MessageHeader.CreateHeader("u", "fuck", "admin");
            var hdPassWord = MessageHeader.CreateHeader("p", "fuck", "123");
            request.Headers.Add(hdUserName);
            request.Headers.Add(hdPassWord);

            return request.ToString();
        }

        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            $"服务器端：接收到的请求:{request.ToString()}".AddBusinessInfoLog();

            // 栓查验证信息  
            var un = request.Headers.GetHeader<string>("u", "fuck");
            var ps = request.Headers.GetHeader<string>("p", "fuck");
            if (un == "admin" && ps == "abcd")
            {
                //
            }
            else
            {
                throw new Exception("验证失败，滚吧！");
            }
            return $"将被传入方法{nameof(IDispatchMessageInspector.BeforeSendReply)}的correlationState参数";
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
            $"服务器即将作出以下回复:{reply.ToString()}".AddBusinessInfoLog();
            return;
        }
    }

    /// <summary>
    /// 测试 可以拦截消息
    /// </summary>
    public class MyEndPointBehavior<T> : IEndpointBehavior
        where T : IClientMessageInspector, IDispatchMessageInspector, new()
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // 不需要  
            return;
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // 植入“偷听器”客户端  
            clientRuntime.ClientMessageInspectors.Add(new T());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // 植入“偷听器” 服务器端  
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new T());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // 不需要  
            return;
        }
    }
}
