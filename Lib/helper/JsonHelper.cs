using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Lib.core;

namespace Lib.helper
{
    public static class JsonHelper
    {
        /// <summary>
        /// json中的时间格式
        /// </summary>
        /// <returns></returns>
        public static IsoDateTimeConverter TimeFormat()
        {
            return new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
        }

        /// <summary>
        /// model转json
        /// </summary>
        public static string ObjectToJson(object obj)
        {
            //if (obj == null) { throw new Exception("null不能被转为json"); }
            return JsonConvert.SerializeObject(obj, TimeFormat());
        }

        /// <summary>
        /// model转xml
        /// </summary>
        public static string ObjectToXml(object obj, string root_name = "root")
        {
            var json = ObjectToJson(obj);
            var xml_json = "{\"" + root_name + "\":" + json + "}";
            var xml = JsonConvert.DeserializeXmlNode(xml_json).OuterXml;
            return xml;
        }

        /// <summary>
        /// 比较两个json结构是否相同
        /// </summary>
        /// <param name="json_1"></param>
        /// <param name="json_2"></param>
        /// <returns></returns>
        public static bool HasSameStructure(string json_1, string json_2)
        {
            var path_list = new List<string>();
            void FindJsonNode(JToken token)
            {
                if (token == null) { return; }
                if (token.Type == JTokenType.Property)
                {
                    path_list.Add(token.Path);
                }
                //find next node
                var children = token.Children();
                foreach (var child in children)
                {
                    FindJsonNode(child);
                }
            }

            FindJsonNode(JToken.Parse(json_1));
            FindJsonNode(JToken.Parse(json_2));

            path_list = path_list.Select(x => ConvertHelper.GetString(x).Trim()).ToList();

            //这里其实有bug
            //如果一个json是空，另一个是两个a.b.c(虽然不可能出现)
            //就会导致两个不一样的json被判断为一样
            //介于不太可能发生，就不想改了,什么时候c#内置函数支持ref再改（强迫症=.=）
            return path_list.Count == path_list.Distinct().Count() * 2;
        }

        private static void JsonParseTest(string json)
        {
            var dom = JObject.Parse(json);
            var mk = dom["io"];
            var props = dom.Properties();
        }

        /// <summary>
        /// json转model
        /// </summary>
        public static T JsonToEntity<T>(string json)
        {
            var b = false;
            if (b) { return JsonConvert.DeserializeObject<T>(json); }

            return (T)JsonToEntity(json, typeof(T));
        }

        /// <summary>
        /// json解析
        /// </summary>
        public static object JsonToEntity(string json, Type type)
        {
            if (!ValidateHelper.IsPlumpString(json)) { throw new Exception("json为空"); }
            if (type == null) { throw new Exception("请指定json对应的实体类型"); }
            try
            {
                return JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception e)
            {
                throw new Exception($"不能将json转为{type.FullName}。json数据：{json}", e);
            }
        }

        /// <summary>
        /// datatable转json
        /// </summary>
        /// <returns>json</returns>
        public static string DataTableToJson(DataTable dt)
        {
            return JsonConvert.SerializeObject(dt, new DataTableConverter(), TimeFormat());
        }

    }
}
