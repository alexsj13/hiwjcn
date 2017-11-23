using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace Hiwjcn.SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var appServer = new AppServer();

            //服务器端口  
            int port = 2000;

            //设置服务监听端口  
            if (!appServer.Setup(port))
            {
                Console.WriteLine("端口设置失败!");
                Console.ReadKey();
                return;
            }

            //新连接事件  
            appServer.NewSessionConnected += new SessionHandler<AppSession>((session) =>
            {
                session.Send("welcome");
            });

            //收到消息事件  
            appServer.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>((session, request_info) =>
            {
                if (request_info.Key == "xx")
                {
                    session.Close();
                }
            });

            //连接断开事件  
            appServer.SessionClosed += new SessionHandler<AppSession, CloseReason>((session, value) =>
            {
                session.Send("bye");
            });

            //启动服务  
            if (!appServer.Start())
            {
                Console.WriteLine("启动服务失败!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("启动服务成功，输入exit退出!");

            while (true)
            {
                var str = Console.ReadLine();
                if (str.ToLower().Equals("exit"))
                {
                    break;
                }
            }

            Console.WriteLine();

            //停止服务  
            appServer.Stop();

            Console.WriteLine("服务已停止，按任意键退出!");
            Console.ReadKey();
        }
    }
}
