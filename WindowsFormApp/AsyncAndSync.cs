using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Lib.extension;
using StackExchange.Redis;

namespace WindowsFormApp
{
    public partial class AsyncAndSync : Form
    {
        public AsyncAndSync()
        {
            InitializeComponent();
        }

        private void AsyncAndSync_Load(object sender, EventArgs e)
        {
            //
        }

        /// <summary>
        /// 不是异步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            Func<string> func = () =>
            {
                Thread.Sleep(3000);
                return "123";
            };

            var msg = await Task.FromResult(func());
            MessageBox.Show(msg);
        }
        /// <summary>
        /// 不是异步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button2_Click(object sender, EventArgs e)
        {
            Func<Task<string>> func = () =>
            {
                Thread.Sleep(3000);
                return Task.FromResult("123");
            };

            var msg = await func();
            MessageBox.Show(msg);
        }
        /// <summary>
        /// 是异步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button3_Click(object sender, EventArgs e)
        {
            Func<Task<string>> func = async () =>
            {
                await Task.Delay(3000);
                return await Task.FromResult("123");
            };

            var msg = await func();
            MessageBox.Show(msg);
        }

        /// <summary>
        /// 是异步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button4_Click(object sender, EventArgs e)
        {
            Func<Task<string>> func = () => Task.Run(() =>
            {
                Thread.Sleep(3000);
                return Task.FromResult("123");
            });

            var msg = await func();
            MessageBox.Show(msg);
        }

        private Task<int> xx()
        {
            var result = new TaskCompletionSource<int>();
            try
            {
                Thread.Sleep(3000);
                result.SetResult(123);
            }
            catch (Exception e)
            {
                result.SetException(e);
            }
            return result.Task;
        }
        private async Task<int> xxAsync()
        {
            return await xx();
        }
        /// <summary>
        /// 假异步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button5_Click(object sender, EventArgs e)
        {
            var data = await xxAsync();
            MessageBox.Show(data.ToString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var pool = default(ConnectionMultiplexer);
            try
            {
                pool = ConnectionMultiplexer.Connect("netscaler.ad.tuhu.cn:6379");

                MessageBox.Show("ok");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.GetInnerExceptionAsJson());
            }
            finally
            {
                pool?.Dispose();
            }
        }
    }
}
