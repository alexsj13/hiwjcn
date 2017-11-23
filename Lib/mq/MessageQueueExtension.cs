using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Lib.helper;
using RabbitMQ.Client.Events;
using Lib.extension;
using Polly;
using Lib.data;

namespace Lib.mq
{
    public static class MessageQueueExtension
    {
        public static string GetExchangeTypeName(this ExchangeTypeEnum type)
        {
            switch (type)
            {
                case ExchangeTypeEnum.direct:
                    return ExchangeType.Direct;
                case ExchangeTypeEnum.fanout:
                    return ExchangeType.Fanout;
                case ExchangeTypeEnum.topic:
                    return ExchangeType.Topic;
                case ExchangeTypeEnum.headers:
                    return ExchangeType.Headers;
            }
            throw new NotSupportedException();
        }

        public static T GetMessage_<T>(this BasicDeliverEventArgs args) =>
            SerializeHelper.Instance.Deserialize<T>(args.Body);

        public static MessageWrapper<T> GetWrapperMessage<T>(this BasicDeliverEventArgs args) =>
            args.GetMessage_<MessageWrapper<T>>();

        public static byte[] DataToWrapperMessageBytes<T>(this T data)
        {
            return Encoding.UTF8.GetBytes(new MessageWrapper<T>() { Data = data }.ToJson());
        }

        public static void BasicAck_(this IModel channel, BasicDeliverEventArgs args)
        {
            channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
        }

        /// <summary>
        /// 添加默认设置项
        /// </summary>
        public static void ExchangeDeclare_(this IModel channel,
            string exchange_name, ExchangeTypeEnum exchange_type,
            bool durable = true, bool auto_delete = false, bool is_delay = false)
        {
            if (is_delay)
            {
                var args = new Dictionary<string, object>()
                {
                    ["x-delayed-type"] = exchange_type.GetExchangeTypeName()
                };
                channel.ExchangeDeclare(exchange_name, "x-delayed-message", durable, auto_delete, args);
            }
            else
            {
                channel.ExchangeDeclare(exchange_name, exchange_type.GetExchangeTypeName(), durable, auto_delete, null);
            }
        }

        /// <summary>
        /// 添加默认设置项
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static QueueDeclareOk X_QueueBind(this IModel channel,
            string queueName, string exchangeName, string routingKey, IDictionary<string, object> arguments)
        {
            var queue = channel.QueueDeclare(queueName, true, false, false, arguments);
            channel.QueueBind(queueName, exchangeName, routingKey);

            return queue;
        }

        /// <summary>
        /// 发送队列
        /// </summary>
        public static void Send<T>(this IModel channel,
            string routeKey, T data,
            string exchangeName = "", bool save_to_disk = true)
        {
            //数据
            var wrapperdata = data.DataToWrapperMessageBytes();

            var properties = channel.CreateBasicProperties();
            properties.Priority = (byte)MessagePriority.Hight;
            properties.Persistent = save_to_disk;
            //etc
            //string exchange, string routingKey, IBasicProperties basicProperties, byte[] body
            channel.BasicPublish(exchangeName, routeKey, properties, wrapperdata);
        }
    }
}
