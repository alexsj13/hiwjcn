using Lib.helper;
using Lib.ioc;
using Lib.data;
using System;
using System.Linq;
using Lib.extension;

namespace Lib.events
{
    public static class EventPublisherExtensions
    {
        public static void EntityInserted<T>(this IEventPublisher eventPublisher, T entity) where T : IDBTable
        {
            eventPublisher.Publish(new EntityInserted<T>(entity));
        }

        public static void EntityUpdated<T>(this IEventPublisher eventPublisher, T entity) where T : IDBTable
        {
            eventPublisher.Publish(new EntityUpdated<T>(entity));
        }

        public static void EntityDeleted<T>(this IEventPublisher eventPublisher, T entity) where T : IDBTable
        {
            eventPublisher.Publish(new EntityDeleted<T>(entity));
        }
    }

    /// <summary>
    /// Evnt publisher
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        /// <summary>
        /// Publish to cunsumer
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="x">Event consumer</param>
        /// <param name="eventMessage">Event message</param>
        protected virtual void PublishToConsumer<T>(IConsumer<T> x, T eventMessage)
        {
            try
            {
                x.HandleEvent(eventMessage);
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }
        }

        /// <summary>
        /// Publish event
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="eventMessage">Event message</param>
        public virtual void Publish<T>(T eventMessage)
        {
            if (!AppContext.IsRegistered<IConsumer<T>>())
            {
                $"无法触发事件，没有在ioc中注册{typeof(IConsumer<T>)}的实例".AddBusinessInfoLog();
                return;
            }
            AppContext.Scope(x =>
            {
                var subscriptions = x.ResolveAll<IConsumer<T>>();
                foreach (var sub in subscriptions)
                {
                    PublishToConsumer(sub, eventMessage);
                }
                return true;
            });
        }

    }
}
