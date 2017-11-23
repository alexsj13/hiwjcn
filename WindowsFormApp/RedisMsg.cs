using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib.core;
using Lib.data;
using Lib.helper;
using System.Threading;
using Lib.data.redis;

namespace WindowsFormApp
{
    public partial class RedisMsg : Form
    {
        public RedisMsg()
        {
            InitializeComponent();
        }

        RedisHelper client = new RedisHelper(dbNum: 1);
        Task t = null;

        private void RedisMsg_Load(object sender, EventArgs e)
        {
            client.Subscribe("wj_pub", (channel, value) =>
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(value);
                }));
            });
            t = Task.Run(() =>
             {
                 while (true)
                 {
                     var data = client.ListRightPop<string>("wj_list");
                     MessageBox.Show(data);
                     Thread.Sleep(5000);
                 }
             });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            client.UnsubscribeAll();
            t.Dispose();
            base.OnClosing(e);
        }

    }
}
