using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Lib.extension;
using RabbitMQ.Client;
using Polly;
using System.Linq;
using Lib.helper;
using Lib.data;
using System.Threading.Tasks;

namespace Lib.mq.rabbitmq
{
    /// <summary>消息发送者</summary>
    public class RabbitMqProducer : IMessageQueueProducer
    {
        private readonly IModel _channel;
        private readonly string _exchange_name;
        private readonly bool _persistent;

        public RabbitMqProducer(IModel channel, string exchangeName, bool persistent = true)
        {
            this._channel = channel;
            this._exchange_name = exchangeName;
            this._persistent = persistent;
        }

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        public virtual void Send(string to, object message) => Send(to, message, CreateBasicProperties(null, null));

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="priority">优先级</param>
        public virtual void Send(string to, object message, MessagePriority priority) => Send(to, message, CreateBasicProperties(priority, null));

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="delay">延迟毫秒数</param>
        public virtual void Send(string to, object message, long delay)
        {
            if (delay > 0)
            {
                Send(to, message, new Dictionary<string, object> { ["x-delay"] = delay });
            }
            else
            {
                Send(to, message);
            }
        }

        /// <summary>发送消息</summary>
        /// <param name="to">routingKey</param>
        /// <param name="message">消息</param>
        /// <param name="priority">优先级</param>
        /// <param name="delay">延迟毫秒数</param>
        public virtual void Send(string to, object message, MessagePriority priority, long delay)
        {
            if (delay > 0)
                Send(to, message, priority, new Dictionary<string, object> { { "x-delay", delay } });
            else
                Send(to, message, priority);
        }

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, IDictionary<string, object> properties) =>
            Send(routingKey, message, CreateBasicProperties(null, properties));

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, MessagePriority? priority, IDictionary<string, object> properties) =>
            Send(routingKey, message, CreateBasicProperties(priority, properties));

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, long delay, IDictionary<string, object> properties) =>
            Send(routingKey, message, null, delay, properties);

        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, MessagePriority? priority, long delay, IDictionary<string, object> properties)
        {
            if (delay > 0)
            {
                var basicProperties = CreateBasicProperties(priority, properties);

                if (basicProperties.Headers == null)
                    basicProperties.Headers = new Dictionary<string, object>();

                basicProperties.Headers["x-delay"] = delay;

                Send(routingKey, message, basicProperties);
            }
            else
                Send(routingKey, message, properties);
        }

        private IBasicProperties CreateBasicProperties(MessagePriority? priority, IDictionary<string, object> properties)
        {
            var basicProperties = _channel.CreateBasicProperties();

            if (this._persistent)
            {
                basicProperties.DeliveryMode = (byte)DeliveryModeEnum.Persistent;
                basicProperties.Persistent = true;
            }
            else
            {
                basicProperties.DeliveryMode = (byte)DeliveryModeEnum.NonPersistent;
                basicProperties.Persistent = false;
            }

            if (priority != null)
            {
                basicProperties.Priority = (byte)priority;
            }

            if (ValidateHelper.IsPlumpDict(properties))
            {
                basicProperties.Headers = new Dictionary<string, object>(properties);
            }

            return basicProperties;
        }


        /// <summary>发送消息</summary>
        public virtual void Send(string routingKey, object message, IBasicProperties basicProperties)
        {
            var bs = SerializeHelper.Instance.Serialize(message);
            this._channel.BasicPublish(this._exchange_name, routingKey, basicProperties, bs);
        }

        public void SendMessage<T>(string routeKey, T message,
            DeliveryModeEnum? deliver_mode = null, MessagePriority? priority = null,
            TimeSpan? delay = null, IDictionary<string, object> properties = null)
        {
            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.DeliveryMode = (byte)DeliveryModeEnum.NonPersistent;
            basicProperties.Persistent = false;

            var header = new Dictionary<string, object>();
            if (ValidateHelper.IsPlumpDict(properties))
            {
                header.AddDict(properties.ToDictionary(x => x.Key, x => x.Value));
            }

            //持久化
            if (this._persistent)
            {
                basicProperties.DeliveryMode = (byte)DeliveryModeEnum.Persistent;
                basicProperties.Persistent = true;
            }
            //优先级
            if (priority != null)
            {
                basicProperties.Priority = (byte)priority;
            }
            //延迟消息
            if (delay != null)
            {
                header["x-delay"] = Math.Abs((long)delay.Value.TotalMilliseconds);
            }

            if (ValidateHelper.IsPlumpDict(header))
            {
                basicProperties.Headers = new Dictionary<string, object>(header);
            }

            var bs = SerializeHelper.Instance.Serialize(message);
            this._channel.BasicPublish(this._exchange_name, routeKey, basicProperties, bs);
        }

        public async Task SendMessageAsync<T>(string routeKey, T message,
            DeliveryModeEnum? deliver_mode = null, MessagePriority? priority = null,
            TimeSpan? delay = null, IDictionary<string, object> properties = null)
        {
            this.SendMessage(routeKey, message, deliver_mode, priority, delay, properties);
            await Task.FromResult(1);
        }

    }
}
