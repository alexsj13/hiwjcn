using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Lib.core;
using Lib.helper;
using Lib.net;
using System.Threading;

namespace WindowsFormApp
{
    public partial class Form1 : Form
    {
        private List<ThreadWrapper> list = null;

        public Form1()
        {
            InitializeComponent();
            list = new List<ThreadWrapper>();
            this.Text = "ok";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            ThreadWrapper.CloseAndRemove(ref list);
            base.OnClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var url = ConvertHelper.GetString(textBox1.Text);
                var sp = url.Split('?').Where(x => ValidateHelper.IsPlumpString(x)).ToArray();
                if (sp.Length != 2)
                {
                    MessageBox.Show("URL格式不正确");
                    return;
                }
                var sign_key = "www_qipeilong_cn";
                var dict = new SortedDictionary<string, string>(new MyStringComparer());
                foreach (var p in sp[1].Split('&'))
                {
                    var kv = ConvertHelper.GetString(p).Split('=');
                    if (!(kv.Length == 1 || kv.Length == 2)) { continue; }
                    if (!ValidateHelper.IsPlumpString(kv[0]))
                    {
                        continue;
                    }
                    if (kv.Length == 1)
                    {
                        kv = new string[] { kv[0], string.Empty };
                    }
                    if (kv[0].ToLower() == "sign" || kv[0].Length <= 0) { continue; }
                    if (kv[0]?.Length > 32 || kv[1]?.Length > 32) { continue; }
                    dict[kv[0]] = kv[1];
                }
                var strdata = string.Join("&", dict.Select(x => x.Key + "=" + x.Value));
                strdata += sign_key;
                strdata = strdata.ToLower();

                var md5 = ConvertHelper.GetString(SecureHelper.GetMD5(strdata)).ToUpper();
                dict["sign"] = md5;
                strdata = string.Join("&", dict.Select(x => x.Key + "=" + x.Value));

                list = list.Where(x => !x.ThreadIsStoped()).ToList();
                var tt = new ThreadWrapper();
                tt.OnStarted = () =>
                {
                    this.Invoke(new Action(() => { this.button1.Enabled = false; }));
                    return true;
                };
                tt.OnFinished = () =>
                {
                    this.Invoke(new Action(() => { this.button1.Enabled = true; }));
                    return true;
                };
                tt.Run((run) =>
                {
                    try
                    {
                        var start = DateTime.Now;
                        var p = dict.ToDictionary(x => x.Key, x => x.Value);
                        var json = HttpClientHelper.Post(sp[0], p);
                        this.Invoke(new Action(() =>
                        {
                            Clipboard.SetDataObject(json);
                            this.textBox2.Text = $"{sp[0]}?{Com.DictToUrlParams(p)}======结果已复制，耗时：{(DateTime.Now - start).TotalMilliseconds}毫秒";
                        }));
                    }
                    catch (Exception err)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(err.Message);
                        }));
                    }
                    return true;
                });
                list.Add(tt);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("start");
            await Task.Run(() =>
            {
                Thread.Sleep(5000);
            });
            MessageBox.Show("end");
        }

        /// <summary>
        /// 异步调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;

            Func<string, int> func = (s) =>
            {
                Thread.Sleep(5000);
                return s.Length;
            };
            func.BeginInvoke("123", (res) =>
            {
                while (!res.IsCompleted) { Thread.Sleep(100); }
                var len = func.EndInvoke(res);
                MessageBox.Show(len.ToString());
                this.Invoke(new Action(() =>
                {
                    button3.Enabled = true;
                }));
            }, null);
        }
    }
}
