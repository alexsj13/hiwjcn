using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Lib.helper;

namespace WindowsFormApp
{
    public partial class NettyClient : Form
    {
        public NettyClient()
        {
            InitializeComponent();
        }

        private readonly MultithreadEventLoopGroup group = new MultithreadEventLoopGroup();
        private IChannel channel;

        private async void NettyClient_Load(object sender, EventArgs e)
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var line = channel.Pipeline;

                    line.AddLast(new DelimiterBasedFrameDecoder(8192, Delimiters.LineDelimiter()));
                    line.AddLast(new StringEncoder(), new StringDecoder());
                    line.AddLast(new ChatClientHandler(this));
                }));

            this.channel = await bootstrap.ConnectAsync(IPAddress.Parse("127.0.0.1"), 8080);
        }

        private async Task CloseAll()
        {
            await this.channel?.CloseAsync();
            await this.group.ShutdownGracefullyAsync();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            try
            {
                await this.CloseAll();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            base.OnClosing(e);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var msg = Com.GetUUID() + "\n";
            await this.channel.WriteAndFlushAsync(msg);

            msg = DateTime.Now.ToString() + "\n";
            await this.channel.WriteAndFlushAsync(msg);

            msg = "=========\n";
            await this.channel.WriteAndFlushAsync(msg);
        }
    }

    public class ChatClientHandler : SimpleChannelInboundHandler<string>
    {
        private readonly Form fm;
        public ChatClientHandler(Form fm)
        {
            this.fm = fm;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, string msg)
        {
            MessageBox.Show(msg);
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
