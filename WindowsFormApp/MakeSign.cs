using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib.extension;
using Lib.core;
using Lib.helper;
using Lib.net;

namespace WindowsFormApp
{
    public partial class MakeSign : Form
    {
        public MakeSign()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.button1.Enabled = false;

                var sign_req_key = "sign";
                var url = ConvertHelper.GetString(this.textBox1.Text);
                if (!url.IsURL())
                {
                    MessageBox.Show("请输入URL");
                    return;
                }
                var param = url.ExtractUrlParams();
                var sign = param.GetSign("www_qipeilong_cn", sign_req_key);
                param[sign_req_key] = sign.sign;

                var req_url = $"{url.Split('?')[0]}?{param.ToUrlParam()}";
                var json = await HttpClientHelper.PostAsync(req_url, null);

                this.textBox4.Text = sign.sign_data;
                this.textBox3.Text = param.GetValueOrDefault(sign_req_key);
                this.textBox2.Text = json;
                Clipboard.SetText(json);
                MessageBox.Show("结果已经复制");
            }
            catch (Exception err)
            {
                this.textBox4.Text = string.Empty;
                this.textBox3.Text = string.Empty;
                this.textBox2.Text = string.Empty;
                MessageBox.Show(err.Message);
            }
            finally
            {
                this.button1.Enabled = true;
            }
        }
    }
}
