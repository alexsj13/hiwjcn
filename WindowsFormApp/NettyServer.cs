using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Codecs;
using System.Net;

namespace WindowsFormApp
{
    public partial class NettyServer : Form
    {
        public NettyServer()
        {
            InitializeComponent();
        }

        private readonly MultithreadEventLoopGroup boss = new MultithreadEventLoopGroup(1);
        private readonly MultithreadEventLoopGroup worker = new MultithreadEventLoopGroup();
        private IChannel channel;

        private async void NettyServer_Load(object sender, EventArgs e)
        {
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(boss, worker)
                .Channel<TcpServerSocketChannel>()
                //.Option(ChannelOption.SoKeepalive, true)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(c =>
                {
                    var line = c.Pipeline;
                    line.AddLast(new DelimiterBasedFrameDecoder(8192, Delimiters.LineDelimiter()));
                    line.AddLast(new StringEncoder(), new StringDecoder());
                    line.AddLast(new ChatServerHandler(this));
                }));
            this.channel = await bootstrap.BindAsync(IPAddress.Parse("127.0.0.1"), 9900);
        }

        private async Task CloseAll()
        {
            await this.channel?.CloseAsync();
            await this.boss.ShutdownGracefullyAsync();
            await this.worker.ShutdownGracefullyAsync();
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                await this.CloseAll();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            base.OnFormClosing(e);
        }
    }

    public class ChatServerHandler : SimpleChannelInboundHandler<string>
    {
        private readonly Form fm;
        public ChatServerHandler(Form fm)
        {
            this.fm = fm;
        }

        static volatile IChannelGroup group;

        public override void ChannelActive(IChannelHandlerContext context)
        {
            if (group == null)
            {
                lock (this)
                {
                    if (group == null)
                    {
                        group = new DefaultChannelGroup(context.Executor);
                    }
                }
            }

            context.WriteAndFlushAsync("welcome\n");

            group.Add(context.Channel);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, string msg)
        {

        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}
