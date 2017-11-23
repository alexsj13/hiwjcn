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
using Lib.extension;

namespace Lib.data.elasticsearch
{
    /// <summary>
    /// ES例子
    /// </summary>
    public class EsExample
    {
        private QueryContainer BuildQuery(SearchParamModel model)
        {
            var temp = new ProductListV2();
            var qc = new QueryContainer();
            {
                var traderlist = new List<string>();
                if (!ValidateHelper.IsPlumpString(model.province))
                {
                    throw new Exception("缺少区域信息");
                }
                if (ValidateHelper.IsPlumpString(model.trader))
                {
                    if (traderlist.Contains(model.trader))
                    {
                        traderlist.Clear();
                        traderlist.Add(model.trader);
                    }
                    else
                    {
                        traderlist.Clear();
                    }
                }
                if (!ValidateHelper.IsPlumpList(traderlist))
                {
                    traderlist = new List<string>() { "构造一个不可能存在的值" };
                }
                qc = qc && new TermsQuery() { Field = nameof(temp.TraderId), Terms = traderlist };
            }
            var idlist = new string[] { };
            if (!new string[] { "2", "4" }.Contains(model.CustomerType))
            {

                qc = qc && (!new TermsQuery() { Field = nameof(temp.UKey), Terms = idlist });
            }
            else
            {
                qc = qc && (!new TermsQuery() { Field = nameof(temp.UKey), Terms = idlist });
            }
            if (ValidateHelper.IsPlumpString(model.brand))
            {
                var brand_sp = ConvertHelper.GetString(model.brand).Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToArray();
                qc = qc && new TermsQuery() { Field = nameof(temp.BrandId), Terms = brand_sp };
            }
            if (ValidateHelper.IsPlumpString(model.catalog))
            {
                qc = qc && (new TermQuery() { Field = nameof(temp.PlatformCatalogId), Value = model.catalog }
                || new TermsQuery() { Field = nameof(temp.PlatformCatalogIdList), Terms = new object[] { model.catalog } }
                || new TermsQuery() { Field = nameof(temp.ShowCatalogIdList), Terms = new object[] { model.catalog } });
            }
            if (model.min_price >= 0)
            {
                qc = qc && new NumericRangeQuery() { Field = nameof(temp.SalesPrice), GreaterThanOrEqualTo = (double)model.min_price };
            }
            if (model.max_price >= 0)
            {
                qc = qc && new NumericRangeQuery() { Field = nameof(temp.SalesPrice), LessThanOrEqualTo = (double)model.max_price };
            }

            new GeoDistanceQuery() { };
            qc = qc && new GeoDistanceRangeQuery()
            {
                Field = "Location",
                Location = new GeoLocation(32, 43),
                LessThanOrEqualTo = Distance.Kilometers(1)
            };

            try
            {
                if (!ValidateHelper.IsPlumpString(model.attr)) { model.attr = "[]"; }
                var attr_list = model.attr.JsonToEntity<List<AttrParam>>();
                /*
                 if (ValidateHelper.IsPlumpList(attr_list))
                {
                    var attr_query = new QueryContainer();
                    foreach (var attr in attr_list)
                    {
                        attr_query = attr_query || new TermQuery() { Field = $"{nameof(template.ProductAttributes)}.{attr.UID}", Value = attr.value };
                    }
                    qc = qc && new NestedQuery() { Path = nameof(template.ProductAttributes), Query = attr_query };
                }
                 */
                if (ValidateHelper.IsPlumpList(attr_list))
                {
                    //qc = qc && new TermsQuery() { Field = nameof(temp.ProductAttributes), Terms = attr_list.Select(attr => $"{attr.UID}@$@{attr.value}") };
                    foreach (var attr_key in attr_list.Select(x => x.UID).Distinct())
                    {
                        qc = qc && new TermsQuery() { Field = nameof(temp.ProductAttributes), Terms = attr_list.Where(x => x.UID == attr_key).Select(attr => $"{attr.UID}@$@{attr.value}") };
                    }
                }
            }
            catch { }
            if (model.isGroup)
            {
                qc = qc && new TermQuery() { Field = nameof(temp.IsGroup), Value = 1 };
            }
            if (ValidateHelper.IsPlumpString(model.qs))
            {
                qc = qc && (new MatchQuery() { Field = nameof(temp.ShopName), Query = model.qs, Operator = Operator.Or, MinimumShouldMatch = "100%" }
                || new MatchQuery() { Field = nameof(temp.SeachTitle), Query = model.qs, Operator = Operator.Or, MinimumShouldMatch = "100%" });
            }

            qc = qc && new TermQuery() { Field = nameof(temp.PAvailability), Value = 1 };
            qc = qc && new TermQuery() { Field = nameof(temp.UpAvailability), Value = 1 };
            qc = qc && new TermQuery() { Field = nameof(temp.PIsRemove), Value = 0 };
            qc = qc && new TermQuery() { Field = nameof(temp.UpIsRemove), Value = 0 };
            qc = qc && new NumericRangeQuery() { Field = nameof(temp.SalesPrice), GreaterThan = 0 };

            return qc;
        }

        private SortDescriptor<ProductListV2> BuildSort(SearchParamModel model)
        {
            var sort = new SortDescriptor<ProductListV2>();
            sort = sort.Descending(x => x.InventoryStatus);
            if (model.order_rule == "1")
            {
                sort = sort.Descending(x => x.SalesVolume);
            }
            else if (model.order_rule == "2")
            {
                sort = sort.Ascending(x => x.SalesPrice);
            }
            else
            {
                sort = sort.Descending(x => x.SalesPrice);
            }

            //更具坐标排序
            sort = sort.GeoDistance(x => x.Field(f => f.IsGroup).PinTo(new GeoLocation(52.310551, 4.404954)).Ascending());

            return sort;
        }

        private static readonly string ES_PRODUCTLIST_INDEX = ConfigurationManager.AppSettings["ES_PRODUCTLIST_INDEX"];
        private static readonly string ES_SERVERS = ConfigurationManager.AppSettings["ES_SERVERS"];

        private static readonly ConnectionSettings setting = new ConnectionSettings(new SniffingConnectionPool(ES_SERVERS.Split('|', ',').Select(x => new Uri(x)))).DisableDirectStreaming(true);

        private static void PrepareES(Func<ElasticClient, bool> func)
        {
            var client = new ElasticClient(setting);
            func.Invoke(client);
        }

        private static Dictionary<string, List<KeyedBucket>> GetAggs<T>(ISearchResponse<T> response) where T : class, new()
        {
            return response?.Aggregations?.ToDictionary(
                    x => x.Key,
                    x => (x.Value as BucketAggregate)?.Items.Select(i => (i as KeyedBucket)).Where(i => i != null).ToList()
                    )?.Where(x => ValidateHelper.IsPlumpList(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        public ISearchResponse<ProductListV2> SearchEsProducts(SearchParamModel model)
        {
            ISearchResponse<ProductListV2> response = null;
            var temp = new ProductListV2();
            PrepareES(client =>
            {
                var sd = new SearchDescriptor<ProductListV2>();

                sd = sd.Index(ES_PRODUCTLIST_INDEX);

                sd = sd.Query(q => BuildQuery(model));

                var NAMEOF_ShowCatalogIdList = nameof(temp.ShowCatalogIdList);
                var NAMEOF_BrandId = nameof(temp.BrandId);
                var NAMEOF_ProductAttributes = nameof(temp.ProductAttributes);

                sd = sd.Aggregations(agg => agg
                .Terms(NAMEOF_ShowCatalogIdList, av => av.Field(NAMEOF_ShowCatalogIdList).Size(1000))
                .Terms(NAMEOF_BrandId, av => av.Field(NAMEOF_BrandId).Size(1000))
                .Terms(NAMEOF_ProductAttributes, av => av.Field(NAMEOF_ProductAttributes).Size(1000)));

                sd = sd.Sort(x => BuildSort(model));

                //var range = PagerHelper.GetQueryRange(model.page, model.pagesize);
                //sd = sd.Skip(range[0]).Take(range[1]);
                sd = sd.QueryPage_(model.page, model.pagesize);

                response = client.Search<ProductListV2>(x => sd);

                var mx = response.Aggs.Max("");

                return true;
            });
            if (response == null || !response.IsValid || response.OriginalException != null)
            {
                throw new Exception("ES 挂了");
            }
            return response;
        }

        public void SearchProducts(SearchParamModel model)
        {
            var data = new PagerData<object>();

            var response = SearchEsProducts(model);
            data.ItemCount = (int)(response?.Total ?? 0);
            var datalist = response?.Hits?.Select(x => x as Hit<ProductListV2>).Where(x => x != null).Select(x => x.Source).Where(x => x != null).ToList();

            //聚合的数据
            var aggs = GetAggs(response);
            data.DataList = ConvertHelper.NotNullList(data.DataList);
        }

        /// <summary>
        /// just for example
        /// </summary>
        [ElasticsearchType(IdProperty = "UKey", Name = "ProductList")]
        public class ProductListV2 : IElasticSearchIndex
        {
            [String(Name = "UKey", Index = FieldIndexOption.NotAnalyzed)]
            public string UKey { get; set; }

            [String(Name = "ProductId", Index = FieldIndexOption.NotAnalyzed)]
            public string ProductId { get; set; }

            [String(Name = "TraderId", Index = FieldIndexOption.NotAnalyzed)]
            public string TraderId { get; set; }

            [String(Name = "PlatformCatalogId", Index = FieldIndexOption.NotAnalyzed)]
            public string PlatformCatalogId { get; set; }

            [String(Name = "BrandId", Index = FieldIndexOption.NotAnalyzed)]
            public string BrandId { get; set; }

            [Number(Name = "PAvailability", Index = NonStringIndexOption.NotAnalyzed)]
            public int PAvailability { get; set; }

            [Number(Name = "PIsRemove", Index = NonStringIndexOption.NotAnalyzed)]
            public int PIsRemove { get; set; }

            [Number(Name = "UpAvailability", Index = NonStringIndexOption.NotAnalyzed)]
            public int UpAvailability { get; set; }

            [Number(Name = "UpIsRemove", Index = NonStringIndexOption.NotAnalyzed)]
            public int UpIsRemove { get; set; }

            [String(Name = "UserSku", Index = FieldIndexOption.NotAnalyzed)]
            public string UserSku { get; set; }

            [Number(Name = "IsGroup", Index = NonStringIndexOption.NotAnalyzed)]
            public int IsGroup { get; set; }

            [Number(Name = "UpiId", Index = NonStringIndexOption.NotAnalyzed)]
            public int UpiId { get; set; }

            /// <summary>
            /// 销量
            /// </summary>
            [Number(Name = "SalesVolume", Index = NonStringIndexOption.NotAnalyzed)]
            public int SalesVolume { get; set; }

            /// <summary>
            /// 是否有货
            /// </summary>
            [Number(Name = "InventoryStatus", Index = NonStringIndexOption.NotAnalyzed)]
            public int InventoryStatus { get; set; }


            [Number(Name = "SalesPrice", Index = NonStringIndexOption.NotAnalyzed)]
            public decimal SalesPrice { get; set; }

            [String(Name = "ShopName", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public string ShopName { get; set; }

            [String(Name = "ShopNamePinyin", Analyzer = "pinyin_analyzer", SearchAnalyzer = "pinyin_analyzer")]
            public string ShopNamePinyin { get; set; }

            [String(Name = "SeachTitle", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public string SeachTitle { get; set; }

            [Date(Name = "UpdatedDate")]
            public DateTime UpdatedDate { get; set; }

            [String(Name = "ShowCatalogIdList", Index = FieldIndexOption.NotAnalyzed)]
            public string[] ShowCatalogIdList { get; set; }

            [String(Name = "PlatformCatalogIdList", Index = FieldIndexOption.NotAnalyzed)]
            public string[] PlatformCatalogIdList { get; set; }

            [String(Name = "ProductAttributes", Index = FieldIndexOption.NotAnalyzed)]
            public string[] ProductAttributes { get; set; }

            [GeoPoint(Name = nameof(Location), LatLon = true, GeoHash = true, Validate = true)]
            public GeoLocation Location { get; set; }

            [GeoShape(Name = nameof(Area))]
            public GeoLocation Area { get; set; }
        }

        public class SearchParamModel
        {
            public string qs { get; set; }
            public string trader { get; set; }
            public string brand { get; set; }
            public string catalog { get; set; }
            public string sys_coupon { get; set; }
            public string user_coupon { get; set; }
            public string attr { get; set; }
            public decimal min_price { get; set; }
            public decimal max_price { get; set; }
            public string province { get; set; }
            public string order_rule { get; set; }
            public int page { get; set; }
            public int pagesize { get; set; }
            public bool isSelf { get; set; }
            public bool isPost { get; set; }
            public bool isGroup { get; set; }
            public bool hidePrice { get; set; }
            public string CustomerType { get; set; }
            public string LoginUserID { get; set; }
        }

        public class AttrParam
        {
            public virtual string UID { get; set; }

            public virtual string value { get; set; }
        }







        public const string INDEX_NAME = "comment_index";

        public PagerData<CommentEs> QueryCommentFromEs(
            string user_product_id = null, string user_uid = null, string q = null, int page = 1, int pagesize = 10)
        {
            var data = new PagerData<CommentEs>();
            var client = ElasticsearchClientManager.Instance.DefaultClient.CreateClient();
            var temp = new CommentEs();
            var tag_temp = new TagEs();

            var sd = new SearchDescriptor<CommentEs>();
            sd = sd.Index(INDEX_NAME);

            #region where
            var query = new QueryContainer();
            if (ValidateHelper.IsPlumpString(user_product_id))
            {
                query &= new TermQuery() { Field = nameof(temp.UserProductUID), Value = user_product_id };
            }
            if (ValidateHelper.IsPlumpString(user_uid))
            {
                query &= new TermQuery() { Field = nameof(temp.UserUID), Value = user_uid };
            }
            if (ValidateHelper.IsPlumpString(q))
            {
                query &= new MatchQuery() { Field = nameof(temp.Comment), Query = q, Operator = Operator.Or, MinimumShouldMatch = "100%" };
            }
            sd = sd.Query(_ => query);
            #endregion

            #region order
            var sort = new SortDescriptor<CommentEs>();
            sort = sort.Descending(x => x.CreateTime);
            sd = sd.Sort(_ => sort);
            #endregion

            #region aggs
            sd = sd.Aggregations(x => x
            .Terms("tags", av => av.Field($"{nameof(temp.Tags)}.{nameof(tag_temp.TagName)}").Size(10))
            .Terms("shops", av => av.Field(f => f.TraderUID).Size(10))
            .Average("score", av => av.Field(f => f.Score)));
            #endregion

            #region pager
            sd = sd.QueryPage_(page, pagesize);
            #endregion

            var response = client.Search<CommentEs>(_ => sd);
            response.ThrowIfException();

            data.ItemCount = (int)response.Total;
            data.DataList = response.Documents.ToList();

            var tags_agg = response.Aggs.Terms("tags");
            var shops_agg = response.Aggs.Terms("shops");
            var score_agg = response.Aggs.Average("score");

            return data;
        }

        public void UpdateIndex()
        {
            var client = ElasticsearchClientManager.Instance.DefaultClient.CreateClient();
            client.CreateIndexIfNotExists(INDEX_NAME, x => x.GetCreateIndexDescriptor<CommentEs>());

            var ran = new Random((int)DateTime.Now.Ticks);
            var list = new List<CommentEs>() { };
            var comment_list = new List<string>()
            {
                "测试发表后页面刷新情况","获取AuthorizationCode报error=1是什么问题","GHOST博客怎么安装啊？急等！","我就测试一下有没有限制敏感词汇，我草你大爷","我看看了，我认为！这个IDE是Hbuilder","中国足协的最新处罚，足以体现“只许州官放火，不许百姓点灯”，桩桩事件的药引子自然都是裁判，他们最后又是事件最终的判官，能让所有球员都不畏惧权力吗？能让所有球员不将心中的愤懑发泄到对方球员身上吗？这就是具有中国特色的中超联赛，但愿足协的高官们，别再被舆论强奸了，真正给赛场以公平的环境，包括你们属下的裁判"
            };
            for (var i = 0; i < 10; ++i)
            {
                list.Add(new CommentEs()
                {
                    UID = Com.GetUUID(),
                    Comment = ran.Choice(comment_list),
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    Images = ran.Sample(comment_list, 3).ToArray(),
                    Tags = new TagEs[]
                    {
                        new TagEs(){TagUID=Com.GetUUID(),TagName="技术交流" },
                        new TagEs(){TagUID=Com.GetUUID(),TagName="评论组件" }
                    },
                    TraderUID = Com.GetUUID(),
                    TraderName = Com.GetUUID(),
                    UserUID = Com.GetUUID(),
                    UserProductUID = Com.GetUUID(),
                    Score = ran.RealNext(10),
                    DimensionUID = Com.GetUUID()
                });
            }

            client.AddToIndex(INDEX_NAME, list.ToArray());
        }


        [ElasticsearchType(IdProperty = nameof(UID), Name = nameof(CommentEs))]
        public class CommentEs : IElasticSearchIndex
        {
            [String(Name = nameof(UID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UID { get; set; }

            [String(Name = nameof(Comment), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public virtual string Comment { get; set; }

            [Date(Name = nameof(CreateTime))]
            public virtual DateTime CreateTime { get; set; }

            [Date(Name = nameof(UpdateTime))]
            public virtual DateTime UpdateTime { get; set; }

            [String(Name = nameof(Images), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string[] Images { get; set; }

            [Object(Name = nameof(Tags))]
            public virtual TagEs[] Tags { get; set; }

            [String(Name = nameof(TraderUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TraderUID { get; set; }

            [String(Name = nameof(TraderName), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TraderName { get; set; }

            [String(Name = nameof(UserUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UserUID { get; set; }

            [String(Name = nameof(UserProductUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UserProductUID { get; set; }

            [Number(Name = nameof(Score))]
            public virtual double Score { get; set; }

            [String(Name = nameof(DimensionUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string DimensionUID { get; set; }

        }

        public class TagEs
        {
            [String(Name = nameof(TagUID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TagUID { get; set; }

            [String(Name = nameof(TagName), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string TagName { get; set; }
        }
    }
}
