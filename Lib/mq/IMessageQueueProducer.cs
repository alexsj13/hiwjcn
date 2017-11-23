using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mq
{
    public interface IMessageQueueProducer
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="to"></param>
        /// <param name="message"></param>
        void Send(string to, object message);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="to"></param>
        /// <param name="message"></param>
        /// <param name="priority"></param>
        void Send(string to, object message, MessagePriority priority);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="to"></param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        void Send(string to, object message, long delay);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="to"></param>
        /// <param name="message"></param>
        /// <param name="priority"></param>
        /// <param name="delay"></param>
        void Send(string to, object message, MessagePriority priority, long delay);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeKey"></param>
        /// <param name="message"></param>
        /// <param name="deliver_mode"></param>
        /// <param name="priority"></param>
        /// <param name="delay"></param>
        /// <param name="properties"></param>
        void SendMessage<T>(string routeKey, T message,
            DeliveryModeEnum? deliver_mode = null,
            MessagePriority? priority = null,
            TimeSpan? delay = null,
            IDictionary<string, object> properties = null);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="routeKey"></param>
        /// <param name="message"></param>
        /// <param name="deliver_mode"></param>
        /// <param name="priority"></param>
        /// <param name="delay"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        Task SendMessageAsync<T>(string routeKey, T message,
            DeliveryModeEnum? deliver_mode = null,
            MessagePriority? priority = null,
            TimeSpan? delay = null,
            IDictionary<string, object> properties = null);
    }
}
