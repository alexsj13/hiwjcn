using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Lib.helper;
using Lib.extension;
using Nest;
using System.Configuration;
using Lib.data.elasticsearch;

namespace Lib.log
{
    /// <summary>
    /// nest用法搜索邮箱
    /// </summary>
    public static class ESLogHelper
    {
        public static readonly string IndexName = ConfigurationManager.AppSettings["es_log_index"] ?? "lib_es_log_index";
        private static readonly ESLogLine temp = new ESLogLine();

        /// <summary>
        /// 搜索日志
        /// </summary>
        /// <param name="highlight"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="keyword"></param>
        /// <param name="logger_name"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static async Task<PagerData<ESLogLine, QueryExtData>> Search(
            bool highlight = true,
            DateTime? start = null, DateTime? end = null,
            string keyword = null, string logger_name = null,
            int page = 1, int pagesize = 10)
        {
            var sd = new SearchDescriptor<ESLogLine>();
            sd = sd.Index(IndexName);

            var query = new QueryContainer();
            if (start != null)
            {
                query &= new DateRangeQuery() { Field = nameof(temp.UpdateTime), GreaterThanOrEqualTo = start.Value };
            }
            if (end != null)
            {
                query &= new DateRangeQuery() { Field = nameof(temp.UpdateTime), LessThan = end.Value };
            }
            if (ValidateHelper.IsPlumpString(keyword))
            {
                query &= new MatchQuery()
                {
                    Field = nameof(temp.Message),
                    Query = keyword,
                    Operator = Operator.Or,
                    MinimumShouldMatch = "100%"
                };
            }
            if (ValidateHelper.IsPlumpString(logger_name))
            {
                query &= new TermQuery() { Field = nameof(temp.LoggerName), Value = logger_name };
            }
            //查询条件
            sd = sd.Query(_ => query);
            //聚合
            sd = sd.Aggregations(x =>
            x.Terms(nameof(temp.LoggerName), av => av.Field(nameof(temp.LoggerName)).Size(1000))
            .Terms(nameof(temp.Level), av => av.Field(nameof(temp.Level)).Size(1000))
            .Terms(nameof(temp.Domain), av => av.Field(nameof(temp.Domain)).Size(1000)));
            //高亮
            if (highlight)
            {
                sd = sd.AddHighlightWrapper("<em class='kwd'>", "</em>", x => x.Field(nameof(temp.Message)));
            }
            //排序
            var sort = new SortDescriptor<ESLogLine>();
            sort = sort.Descending(x => x.UpdateTime);
            sort = sort.Descending(Field.Create("_score", boost: null));
            sd = sd.Sort(_ => sort);

            //分页
            sd = sd.QueryPage_(page, pagesize);

            //请求服务器
            var client = new ElasticClient(ElasticsearchClientManager.Instance.DefaultClient);
            var re = await client.SearchAsync<ESLogLine>(_ => sd);

            re.ThrowIfException();

            var data = new PagerData<ESLogLine, QueryExtData>();
            data.ItemCount = (int)re.Total;
            data.DataList = re.Hits.Select(x => x.Source).ToList();
            //聚合数据
            data.ExtData = new QueryExtData();
            data.ExtData.Ass = re.GetAggs();
            data.ExtData.Highlight = re.GetHighlights();

            return data;
        }

        /// <summary>
        /// 查询额外数据
        /// </summary>
        public class QueryExtData
        {
            /// <summary>
            /// 分组聚合
            /// </summary>
            public Dictionary<string, List<KeyedBucket>> Ass { get; set; }

            /// <summary>
            /// 高亮
            /// </summary>
            public List<HighlightHit> Highlight { get; set; }
        }

        /// <summary>
        /// 删除此日期之前的数据
        /// 由定时任务调用，不用异步
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static void DeleteDataBefore(DateTime time)
        {
            var req = new DeleteByQueryRequest<ESLogLine>(IndexName);
            req.Query = new QueryContainer();
            req.Query &= new DateRangeQuery() { Field = nameof(temp.UpdateTime), LessThan = time };
            //delete by query
            var client = new ElasticClient(ElasticsearchClientManager.Instance.DefaultClient);
            var response = client.DeleteByQuery(req);
            response.ThrowIfException();
        }
    }
}
