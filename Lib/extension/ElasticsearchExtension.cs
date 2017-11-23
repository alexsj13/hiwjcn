using Elasticsearch.Net;
using Lib.helper;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using Lib.core;
using System.Threading.Tasks;
using Lib.data.elasticsearch;

namespace Lib.extension
{
    public static class ElasticsearchExtension
    {
        /// <summary>
        /// 如果有错误就抛出异常
        /// </summary>
        /// <param name="response"></param>
        public static T ThrowIfException<T>(this T response) where T : IResponse
        {
            if (!response.IsValid)
            {
                if (response.ServerError?.Error != null)
                {
                    var msg = $@"server errors:{response.ServerError.Error.ToJson()},debug information:{response.DebugInformation}";
                    throw new Exception(msg);
                }
                if (response.OriginalException != null)
                {
                    throw response.OriginalException;
                }
            }
            return response;
        }

        /// <summary>
        /// 设置shards和replicas和model搜索deep
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="shards"></param>
        /// <param name="replicas"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        public static CreateIndexDescriptor GetCreateIndexDescriptor<T>(this CreateIndexDescriptor x,
            int shards = 5, int replicas = 1, int deep = 5)
            where T : class, IElasticSearchIndex
        {
            return x.Settings(s =>
            s.NumberOfShards(shards).NumberOfReplicas(replicas)).Mappings(map => map.Map<T>(m => m.AutoMap(deep)));
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sd"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static SearchDescriptor<T> QueryPage_<T>(this SearchDescriptor<T> sd, int page, int pagesize)
            where T : class, IElasticSearchIndex
        {
            var pager = PagerHelper.GetQueryRange(page, pagesize);
            return sd.Skip(pager.skip).Take(pager.take);
        }

        /// <summary>
        /// 如果索引不存在就创建
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static void CreateIndexIfNotExists(this IElasticClient client, string indexName, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            indexName = indexName.ToLower();

            var exist_response = client.IndexExists(indexName);
            exist_response.ThrowIfException();

            if (exist_response.Exists)
            {
                return;
            }

            var response = client.CreateIndex(indexName, selector);
            response.ThrowIfException();
        }

        /// <summary>
        /// 如果索引不存在就创建
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static async Task CreateIndexIfNotExistsAsync(this IElasticClient client, string indexName, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            indexName = indexName.ToLower();
            var exist_response = await client.IndexExistsAsync(indexName);
            exist_response.ThrowIfException();

            if (exist_response.Exists)
            {
                return;
            }

            var response = await client.CreateIndexAsync(indexName, selector);
            response.ThrowIfException();
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static void DeleteIndexIfExists(this IElasticClient client, string indexName)
        {
            indexName = indexName.ToLower();

            var exist_response = client.IndexExists(indexName);
            exist_response.ThrowIfException();

            if (!exist_response.Exists)
            {
                return;
            }

            var response = client.DeleteIndex(indexName);
            response.ThrowIfException();
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static async Task DeleteIndexIfExistsAsync(this IElasticClient client, string indexName)
        {
            indexName = indexName.ToLower();
            var exist_response = await client.IndexExistsAsync(indexName);
            exist_response.ThrowIfException();

            if (!exist_response.Exists)
            {
                return;
            }

            var response = await client.DeleteIndexAsync(indexName);
            response.ThrowIfException();
        }

        /// <summary>
        /// 添加到索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="data"></param>
        public static void AddToIndex<T>(this IElasticClient client, string indexName, params T[] data)
            where T : class, IElasticSearchIndex
        {
            var bulk = new BulkRequest(indexName)
            {
                Operations = ConvertHelper.NotNullList(data).Select(x => new BulkIndexOperation<T>(x)).ToArray()
            };
            var response = client.Bulk(bulk);

            response.ThrowIfException();
        }

        /// <summary>
        /// 添加到索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task AddToIndexAsync<T>(this IElasticClient client, string indexName, params T[] data) where T : class, IElasticSearchIndex
        {
            var bulk = new BulkRequest(indexName)
            {
                Operations = ConvertHelper.NotNullList(data).Select(x => new BulkIndexOperation<T>(x)).ToArray()
            };
            var response = await client.BulkAsync(bulk);

            response.ThrowIfException();
        }

        /// <summary>
        /// 搜索建议
        /// https://elasticsearch.cn/article/142
        /// </summary>
        public static IDictionary<string, Suggest[]> PhraseSuggest<T>(this IElasticClient client,
            string index,
            Expression<Func<T, object>> targetField, string text,
            string highlight_pre = "<em>", string hightlight_post = "</em>", int size = 20)
            where T : class, IElasticSearchIndex
        {
            var response = client.Suggest<T>(
                x => x.Index(index).Phrase("phrase_suggest",
                f => f.Field(targetField).Text(text)
                .Highlight(h => h.PreTag(highlight_pre).PostTag(hightlight_post)).Size(size)));

            response.ThrowIfException();

            return response.Suggestions;
        }

        /// <summary>
        /// 搜索建议
        /// </summary>
        public static IDictionary<string, Suggest[]> TermSuggest<T>(this IElasticClient client,
            string index,
            Expression<Func<T, object>> targetField, string text, string analyzer = null,
            string highlight_pre = "<em>", string hightlight_post = "</em>", int size = 20)
            where T : class, IElasticSearchIndex
        {
            var sd = new TermSuggesterDescriptor<T>();
            sd = sd.Field(targetField).Text(text);
            if (ValidateHelper.IsPlumpString(analyzer))
            {
                sd = sd.Analyzer(analyzer);
            }
            sd = sd.Size(size);

            var response = client.Suggest<T>(x => x.Index(index).Term("term_suggest", f => sd));

            response.ThrowIfException();

            return response.Suggestions;
        }

        /// <summary>
        /// 搜索建议
        /// </summary>
        public static IDictionary<string, Suggest[]> CompletionSuggest<T>(this IElasticClient client,
            string index,
            Expression<Func<T, object>> targetField, string text, string analyzer = null,
            string highlight_pre = "<em>", string hightlight_post = "</em>", int size = 20)
            where T : class, IElasticSearchIndex
        {
            var sd = new CompletionSuggesterDescriptor<T>();
            sd = sd.Field(targetField).Text(text);
            if (ValidateHelper.IsPlumpString(analyzer))
            {
                sd = sd.Analyzer(analyzer);
            }
            sd = sd.Size(size);

            var response = client.Suggest<T>(x => x.Index(index).Completion("completion_suggest", f => sd));

            response.ThrowIfException();

            return response.Suggestions;
        }

        public static async Task<List<string>> SimpleCompletionSuggest<T>(this IElasticClient client,
            string index,
            string keyword, string analyzer = null, int size = 20)
            where T : CompletionSuggestIndexBase
        {
            var data = new List<string>();
            if (!ValidateHelper.IsPlumpString(keyword))
            {
                return data;
            }

            var sd = new CompletionSuggesterDescriptor<T>();
            sd = sd.Field(f => f.CompletionSearchTitle).Text(keyword);
            if (ValidateHelper.IsPlumpString(analyzer))
            {
                sd = sd.Analyzer(analyzer);
            }
            //允许错4个单词
            sd = sd.Fuzzy(f => f.Fuzziness(Fuzziness.EditDistance(4)));
            sd = sd.Size(size);

            var s_name = "p";

            var response = await client.SuggestAsync<T>(x => x.Index(index).Completion(s_name, f => sd));
            response.ThrowIfException();

            var list = response.Suggestions?[s_name]?.FirstOrDefault()?.Options?.ToList();
            if (!ValidateHelper.IsPlumpList(list))
            {
                return data;
            }
            var sggs = list.OrderByDescending(x => x.Score).Select(x => x.Text);
            data.AddRange(sggs);

            return data.Where(x => ValidateHelper.IsPlumpString(x)).Distinct().ToList();
        }

        /// <summary>
        /// 给关键词添加高亮
        /// </summary>
        public static SearchDescriptor<T> AddHighlightWrapper<T>(this SearchDescriptor<T> sd,
            string pre = "<em>", string after = "</em>",
            params Func<HighlightFieldDescriptor<T>, IHighlightField>[] fieldHighlighters)
            where T : class, IElasticSearchIndex
        {
            if (fieldHighlighters.Length <= 0)
            {
                throw new Exception("关键词高亮，但是没有指定高亮字段");
            }
            return sd.Highlight(x => x.PreTags(pre).PostTags(after).Fields(fieldHighlighters));
        }

        /// <summary>
        /// 获取高亮对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="re"></param>
        /// <returns></returns>
        public static List<HighlightHit> GetHighlights<T>(this ISearchResponse<T> re)
            where T : class, IElasticSearchIndex
        {
            var data = re.Highlights.Select(x => x.Value?.Select(m => m.Value).ToList()).ToList();
            data = data.Where(x => ValidateHelper.IsPlumpList(x)).ToList();

            return data.Reduce((a, b) => ConvertHelper.NotNullList(a).Concat(ConvertHelper.NotNullList(b)).ToList());
        }

        /// <summary>
        /// 获取聚合
        /// 升级到5.0，这个方法不可用，需要改动
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Dictionary<string, List<KeyedBucket>> GetAggs<T>(this ISearchResponse<T> response)
            where T : class, IElasticSearchIndex
        {
            List<KeyedBucket> C(IAggregate x)
            {
                if (x is BucketAggregate _BucketAggregate)
                {
                    var data = new List<KeyedBucket>();
                    foreach (var i in _BucketAggregate.Items)
                    {
                        if (i is KeyedBucket _KeyedBucket)
                        {
                            if (_KeyedBucket.DocCount > 0)
                            {
                                data.Add(_KeyedBucket);
                            }
                        }
                    }
                    return data;
                }
                //老方式
                return (x as BucketAggregate)?.Items?.Select(i => (i as KeyedBucket)).Where(i => i?.DocCount > 0).ToList();
            }
            var aggs = response.Aggregations?.ToDictionary(x => x.Key, x => C(x.Value));
            return aggs.Where(x => ValidateHelper.IsPlumpList(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 开启链接调试
        /// </summary>
        public static ConnectionSettings EnableDebug(this ConnectionSettings setting)
        {
            return setting.DisableDirectStreaming(true);
        }

        /// <summary>
        /// 记录请求信息
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="handlerOrDefault"></param>
        /// <returns></returns>
        public static ConnectionSettings LogRequestInfo(this ConnectionSettings pool, Action<IApiCallDetails> handlerOrDefault = null)
        {
            if (handlerOrDefault == null)
            {
                handlerOrDefault = x =>
                {
                    if (x.OriginalException != null)
                    {
                        x.OriginalException.AddErrorLog();
                    }
                    new
                    {
                        debuginfo = x.DebugInformation,
                        url = x.Uri.ToString(),
                        success = x.Success,
                        method = x.HttpMethod.ToString()
                    }.ToJson().AddBusinessInfoLog();
                };
            }
            return pool.OnRequestCompleted(handlerOrDefault);
        }

        /// <summary>
        /// 创建client
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static IElasticClient CreateClient(this ConnectionSettings pool) => new ElasticClient(pool);

        /// <summary>
        /// 唯一ID
        /// </summary>
        public static DocumentPath<T> ID<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            return DocumentPath<T>.Id(uid).Index(indexName);
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        public static bool DocExist_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var response = client.DocumentExists(client.ID<T>(indexName, uid));
            return response.ThrowIfException().Exists;
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        public static async Task<bool> DocExistAsync_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var response = await client.DocumentExistsAsync(client.ID<T>(indexName, uid));
            return response.ThrowIfException().Exists;
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        public static void UpdateDoc_<T>(this IElasticClient client, string indexName, string uid, T doc) where T : class, IElasticSearchIndex
        {
            var update_response = client.Update(client.ID<T>(indexName, uid), x => x.Doc(doc));
            update_response.ThrowIfException();
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        public static async Task UpdateDocAsync_<T>(this IElasticClient client, string indexName, string uid, T doc) where T : class, IElasticSearchIndex
        {
            var update_response = await client.UpdateAsync(client.ID<T>(indexName, uid), x => x.Doc(doc));
            update_response.ThrowIfException();
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        public static void DeleteDoc_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var delete_response = client.Delete(client.ID<T>(indexName, uid));
            delete_response.ThrowIfException();
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        public static async Task DeleteDocAsync_<T>(this IElasticClient client, string indexName, string uid) where T : class, IElasticSearchIndex
        {
            var delete_response = await client.DeleteAsync(client.ID<T>(indexName, uid));
            delete_response.ThrowIfException();
        }

        /// <summary>
        /// 通过条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="where"></param>
        public static void DeleteByQuery_<T>(this IElasticClient client, string indexName, QueryContainer where) where T : class, IElasticSearchIndex
        {
            var query = new DeleteByQueryRequest<T>(indexName) { Query = where };

            var response = client.DeleteByQuery(query);

            response.ThrowIfException();
        }

        /// <summary>
        /// 通过条件删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static async Task DeleteByQueryAsync_<T>(this IElasticClient client, string indexName, QueryContainer where) where T : class, IElasticSearchIndex
        {
            var query = new DeleteByQueryRequest<T>(indexName) { Query = where };

            var response = await client.DeleteByQueryAsync(query);

            response.ThrowIfException();
        }

        /// <summary>
        /// 根据距离排序
        /// </summary>
        public static SortDescriptor<T> SortByDistance<T>(this SortDescriptor<T> sort,
            Expression<Func<T, object>> field, GeoLocation point, bool desc = false) where T : class, IElasticSearchIndex
        {
            var geo_sort = new SortGeoDistanceDescriptor<T>().PinTo(point);
            if (desc)
            {
                geo_sort = geo_sort.Descending();
            }
            else
            {
                geo_sort = geo_sort.Ascending();
            }
            return sort.GeoDistance(x => geo_sort);
            /*
            if (desc)
            {
                return sort.GeoDistance(x => x.Field(field).PinTo(new GeoLocation(lat, lng)).Descending());
            }
            else
            {
                return sort.GeoDistance(x => x.Field(field).PinTo(new GeoLocation(lat, lng)).Ascending());
            }*/
        }

        /// <summary>
        /// 怎么通过距离筛选，请看源代码
        /// ES空间搜索
        /// </summary>
        /// <param name="qc"></param>
        public static void HowToFilterByDistance(this QueryContainer qc)
        {
            qc = qc && new GeoBoundingBoxQuery()
            {
                Field = "name",
                BoundingBox = new BoundingBox()
                {
                    TopLeft = new GeoLocation(212, 32),
                    BottomRight = new GeoLocation(43, 56)
                }
            };
            qc = qc && new GeoDistanceRangeQuery()
            {
                Field = "Field Name",
                Location = new GeoLocation(32, 43),
                LessThanOrEqualTo = Distance.Kilometers(1)
            };
            qc = qc && new GeoShapeCircleQuery()
            {
                Field = "name",
                Shape = new CircleGeoShape()
                {
                    Coordinates = new GeoCoordinate(324, 535)
                },
                Relation = GeoShapeRelation.Within
            };
        }

        public static void HowToUseAggregationsInES(this SearchDescriptor<EsExample.ProductListV2> sd)
        {
            var agg = new AggregationContainer();
            agg = new SumAggregation("", Field.Create("")) && new AverageAggregation("", Field.Create(""));

            sd = sd.Aggregations(a => a.Max("max", x => x.Field(m => m.IsGroup)));
            sd = sd.Aggregations(a => a.Stats("stats", x => x.Field(m => m.BrandId).Field(m => m.PIsRemove)));

            var response = ElasticsearchClientManager.Instance.DefaultClient.CreateClient().Search<EsExample.ProductListV2>(x => sd);

            var stats = response.Aggs.Stats("stats");
            //etc
        }

        public static void SortWithScripts<T>(this SortDescriptor<T> sort) where T : class, IElasticSearchIndex
        {
            var sd = new SortScriptDescriptor<T>();

            sd = sd.Mode(SortMode.Sum).Type("number");
            var script = "doc['price'].value * params.factor";
            sd = sd.Script(x => x.Inline(script).Lang("painless").Params(new Dictionary<string, object>()
            {
                ["factor"] = 1.1
            }));

            sort.Script(x => sd.Descending());
        }

        /// <summary>
        /// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/function-score-query-usage.html
        /// </summary>
        /// <param name="sd"></param>
        public static void FunctionQuery(this SearchDescriptor<EsExample.ProductListV2> sd)
        {
            var qs = new FunctionScoreQuery()
            {
                Name = "named_query",
                Boost = 1.1,
                Query = new MatchAllQuery { },
                BoostMode = FunctionBoostMode.Multiply,
                ScoreMode = FunctionScoreMode.Sum,
                MaxBoost = 20.0,
                MinScore = 1.0,
                Functions = new List<IScoreFunction>
                {
                    new ExponentialDecayFunction { Origin = 1.0, Decay =    0.5, Field = "", Scale = 0.1, Weight = 2.1 },
                    new GaussDateDecayFunction { Origin = DateMath.Now, Field = "", Decay = 0.5, Scale = TimeSpan.FromDays(1) },
                    new LinearGeoDecayFunction { Origin = new GeoLocation(70, -70), Field = "", Scale = Distance.Miles(1), MultiValueMode = MultiValueMode.Average },
                    new FieldValueFactorFunction
                    {
                        Field = "x", Factor = 1.1,    Missing = 0.1, Modifier = FieldValueFactorModifier.Ln
                    },
                    new RandomScoreFunction { Seed = 1337 },
                    new GaussGeoDecayFunction() { Origin=new GeoLocation(32,4) },
                    new RandomScoreFunction { Seed = "randomstring" },
                    new WeightFunction { Weight = 1.0},
                    new ScriptScoreFunction { Script = new ScriptQuery { File = "x" } }
                }
            };
            sd = sd.Query(x => qs);
            sd = sd.Sort(x => x.Descending(s => s.UpdatedDate));
            sd = sd.Skip(0).Take(10);
            new ElasticClient().Search<EsExample.ProductListV2>(_ => sd);
        }

        public static void UpdateDoc(IElasticClient client)
        {
            //https://stackoverflow.com/questions/42210930/nest-how-to-use-updatebyquery

            var query = new QueryContainer();
            query &= new TermQuery() { Field = "name", Value = "wj" };

            client.UpdateByQuery<EsExample.ProductListV2>(q => q.Query(rq => query).Script(script => script
        .Inline("ctx._source.name = newName;")
        .Params(new Dictionary<string, object>() { ["newName"] = "wj" })));

            //
            client.Update(DocumentPath<EsExample.ProductListV2>.Id(""),
                x => x.Index("").Type<EsExample.ProductListV2>().Doc(new EsExample.ProductListV2() { }));
        }

    }
}
