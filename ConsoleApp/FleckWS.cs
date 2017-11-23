using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;

namespace ConsoleApp
{
    public static class FleckWS
    {
        public static void ws()
        {
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine($"{socket.ConnectionInfo.Id}:Open");
                    var valid = false;
                    if (!valid)
                    {
                        socket.Send(new { close = true, reason = "验证未通过" }.ToJson());
                        socket.Close();
                    }
                };
                socket.OnClose = () => { Console.WriteLine("Close"); };
                socket.OnMessage = async msg => { await socket.Send(msg); };
            });
            Console.ReadLine();
            server.Dispose();
        }
    }
}
