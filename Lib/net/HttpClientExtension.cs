using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.io;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Lib.net
{
    public static class HttpClientExtension
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync_<T>(this HttpClient client, string url, T data) =>
            await client.PostAsJsonAsync(url, data);

        public static void SetCookies(this HttpClientHandler handler, List<Cookie> cookies, Uri uri = null)
        {
            if (handler.CookieContainer == null)
            {
                handler.CookieContainer = new CookieContainer();
            }
            handler.UseCookies = true;
            if (ValidateHelper.IsPlumpList(cookies))
            {
                foreach (var c in cookies)
                {
                    if (uri == null)
                    {
                        handler.CookieContainer.Add(c);
                    }
                    else
                    {
                        handler.CookieContainer.Add(uri, c);
                    }
                }
            }
        }

        public static void AddFile_(this MultipartFormDataContent content, string key, string file_path)
        {
            var bs = File.ReadAllBytes(file_path);
            var name = Path.GetFileName(file_path);
            var content_type = StaticData.MimeTypes.GetMimeType(Path.GetExtension(file_path));
            content.AddFile_(key, bs, name, content_type);
        }

        public static void AddFile_(this MultipartFormDataContent content,
            string key, byte[] bs, string file_name, string content_type)
        {
            var fileContent = new ByteArrayContent(bs);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = key,
                FileName = file_name,
                Size = bs.Length
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(content_type);
            content.Add(fileContent, key);
        }

        public static void AddParam_(this MultipartFormDataContent content, string key, string value)
        {
            content.Add(new StringContent(value), key);
        }

        public static void AddParam_(this MultipartFormDataContent content, IDictionary<string, string> param)
        {
            foreach (var kv in param.ParamNotNull())
            {
                content.AddParam_(kv.Key, kv.Value);
            }
        }

        public static Dictionary<string, string> ParamNotNull(this IDictionary<string, string> param)
        {
            return param.Where(x => ValidateHelper.IsPlumpString(x.Key))
                .ToDictionary(x => ConvertHelper.GetString(x.Key), x => ConvertHelper.GetString(x.Value));
        }

        public static string GetMethodString(this RequestMethodEnum m) => m.ToString();

    }
}
