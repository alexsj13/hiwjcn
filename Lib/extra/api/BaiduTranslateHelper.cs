using Lib.helper;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.net;
using Polly.CircuitBreaker;
using Polly;

namespace Lib.extra.api
{
    public class BaiduTranslateHelper
    {
        /// <summary>
        /// 连续错误10就熔断1分钟
        /// </summary>
        private static readonly CircuitBreakerPolicy p_async =
            Policy.Handle<Exception>().CircuitBreakerAsync(50, TimeSpan.FromMinutes(1));

        #region 百度翻译接口
        class BaiduTransResult
        {
            public virtual string src { get; set; }
            public virtual string dst { get; set; }
        }
        class BaiduTransJson
        {
            public virtual string error_msg { get; set; }
            public virtual string from { get; set; }
            public virtual string to { get; set; }
            public virtual List<BaiduTransResult> trans_result { get; set; }
        }
        public static async Task<string> BaiduTranslate(string q, string from, string to)
        {
            if (!ValidateHelper.IsAllPlumpString(q)) { return q; }
            if (!ValidateHelper.IsAllPlumpString(from, to)) { throw new Exception("from or to is empty"); }

            var appid = "20160923000029191";
            var securityid = "4rjkaBYXiu1IK7QsvBOh";
            var salt = Com.GetRandomNumString(10);
            var md5 = SecureHelper.GetMD5($"{appid}{q}{salt}{securityid}").ToLower();

            //q=apple&from=en&to=zh&appid=2015063000000001&salt=1435660288&sign=f89f9594663708c1605f3d736d01d2d4
            var url = "http://api.fanyi.baidu.com/api/trans/vip/translate";
            var trans = string.Empty;

            var dict = new Dictionary<string, string>();
            dict["q"] = EncodingHelper.UrlEncode(q);
            dict["from"] = from;
            dict["to"] = to;
            dict["appid"] = appid;
            dict["salt"] = salt;
            dict["sign"] = md5;

            var urlparam = $"{url}?{dict.ToUrlParam()}";
            //trans = await HttpClientHelper.GetAsync(urlparam);
            trans = await p_async.ExecuteAsync(async () => await HttpClientHelper.GetAsync(urlparam));
            if (!ValidateHelper.IsPlumpString(trans)) { throw new Exception("翻译失败"); }
            return trans;
        }
        #endregion
    }
}
