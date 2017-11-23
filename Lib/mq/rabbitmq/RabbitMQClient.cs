using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.mq.rabbitmq
{
    public class RabbitMqClient : IDisposable
    {
        private readonly IConnectionFactory _factory;
        private readonly Lazy<IConnection> _connection;

        public RabbitMqClient(string configurationName) : this(RabbitMqSection.FromSection(configurationName)) { }

        public RabbitMqClient(RabbitMqSection configuration)
        {
            this._factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                UseBackgroundThreadsForIO = true,
                HostName = configuration.HostName,
                UserName = configuration.UserName,
                Password = configuration.Password,
                VirtualHost = configuration.VirtualHost,
                ContinuationTimeout = TimeSpan.FromMilliseconds(configuration.ContinuationTimeout),
                HandshakeContinuationTimeout = TimeSpan.FromMilliseconds(configuration.ContinuationTimeout),
                RequestedConnectionTimeout = configuration.SocketTimeout,
                SocketReadTimeout = configuration.SocketTimeout,
                SocketWriteTimeout = configuration.SocketTimeout,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(1)
            };

            _connection = new Lazy<IConnection>(() =>
            {
                var con = _factory.CreateConnection();
                con.ConnectionShutdown += (sender, args) => { };
                con.ConnectionBlocked += (sender, args) => { };
                con.ConnectionUnblocked += (sender, args) => { };
                return con;
            });
        }

        public IConnection Connection => _connection.Value;

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                this.Connection?.Close();
            }
            catch
            { }

            try
            {
                this.Connection?.Dispose();
            }
            catch
            { }

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~RabbitMqClient()
        {
            this.Dispose();
        }
    }
}
