using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.MachineLearning;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using seachdemo.Models;
using System.Text.RegularExpressions;

namespace seachdemo.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ElasticsearchClient _elasticClient;
        private readonly CustomerSearchService _customerSearchService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ElasticsearchClient elasticClient, CustomerSearchService customerSearchService)
        {
            _logger = logger;
            this._elasticClient = elasticClient;
            this._customerSearchService = customerSearchService;
        }

        [HttpPost(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Post(SearchModel model)
        {


            var ret = await _customerSearchService.SearchCustomersWithConditionsAsync(model);
            // 通过mapster，将查询结果映射到CustomerDto
            var customers = ret.Documents.Adapt<List<CustomerDto>>();
            return Ok(customers);
        }

        [HttpPost("with-orders")]
        public async Task<IActionResult> GetCustomersWithOrders(SearchModel model)
        {
            var customersWithOrders = await _customerSearchService.SearchCustomersWithOrdersAsync(model);
            var dtos = customersWithOrders.Adapt<List<CustomerWithOrdersDto>>();
            return Ok(dtos);
        }

        /// <summary>
        /// 使用优化的查询 - 带过滤器
        /// </summary>
        [HttpPost("optimized")]
        public async Task<IActionResult> GetCustomersOptimized(SearchModel model)
        {
            var ret = await _customerSearchService.SearchCustomersWithConditionsAsync(model);

            return Ok(new
            {
                TotalHits = ret.Total,
                MaxScore = ret.MaxScore,
                ElapsedMs = ret.Took,
                Documents = ret.Documents,
                Hits = ret.Hits.Select(h => new
                {
                    Id = h.Id,
                    Score = h.Score,
                    Source = h.Source,
                    Highlights = h.Highlight
                }).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> mget()
        {
            var customerids = new string[]{ "1", "3" };
            
            var docs = await _elasticClient.MultiGetAsync<CustomerDoc>(r => r.Index("customerorder").Ids(customerids));
         
            var ret = new List<CustomerDoc>();
            foreach (var item in docs.Docs)
            {
                var doc= item.Match<CustomerDoc>(r=> { r.Source.Id = r.Id; return r.Source; },e=>null);
                ret.Add(doc);
            }
            var docsDto = ret.Adapt<List<CustomerDto>>();

            return Ok(docsDto);
        }

        /// <summary>
        /// 使用功能评分查询
        /// </summary>
        [HttpPost("function-score")]
        public async Task<IActionResult> GetCustomersFunctionScore(SearchModel model)
        {
            var weight1Score = FunctionScore.WeightScore(2.0);
            weight1Score.Filter = new TermQuery("hobby.keyword")
            {
                Value = "爬山"
            };
            var weight2Score = FunctionScore.WeightScore(1.0);
            weight2Score.Filter = new TermQuery("hobby.keyword")
            {
                Value = "上网"
            };

            var searchRequest = new SearchRequest(Indices.Index("customerorder"))
            {
                // 设置返回大小
                Size = 20,
                // FunctionScore查询
                Query = new FunctionScoreQuery
                {
                    // 基础查询 - 可以是任何查询类型
                    Query = new BoolQuery
                    {
                        Must = new List<Query>
                                {
                                    new MatchQuery("desc")
                                    {
                                         Query=model.DescContent
                                    }
                                }
                    },
                    // 评分函数列表
                    Functions = new List<FunctionScore>
                                {   weight1Score,    
                                    weight2Score,
                                    //FunctionScore.FieldValueFactor(new FieldValueFactorScoreFunction()
                                    //{
                                    //     Field="_id",
                                    //     Factor = 1.0,
                                    //}) ,
                                    //FunctionScore.Gauss(new NumericDecayFunction()
                                    //{
                                    //     Field="_id",
                                    //     MultiValueMode= MultiValueMode.Avg,
                                    //     Placement = new DecayPlacement<double, double>()
                                    //     {  Origin=0,
                                    //        Scale =0.5, // 30天为比例尺
                                    //        Offset =0, // 无偏移
                                    //        Decay = 0.5 // 30天后的评分降为最高分的50%
                                    //     }
                                    //})
                                },

                    // 评分模式 - 如何组合多个函数的分数
                    ScoreMode = FunctionScoreMode.Sum, // 将所有匹配的函数分数相加
                    // 提升模式 - 如何将函数分数与查询分数结合
                    BoostMode = FunctionBoostMode.Multiply, // 将函数分数与查询分数相乘
                    // 全局提升因子
                    Boost = 1.5f
                },
                // 高亮配置
                Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
                    {
                        { "desc", new HighlightField() },
                        { "hobby", new HighlightField() }
                    },
                    PreTags = new[] { "<em>" },
                    PostTags = new[] { "</em>" }
                }
            };

            // 正确执行查询
            var searchResponse = await _elasticClient.SearchAsync<CustomerDoc>(searchRequest);

            // 处理搜索结果
            var results = new List<CustomerDto>();
            if (searchResponse.IsValidResponse)
            {
                foreach (var hit in searchResponse.Hits)
                {
                    var customer = hit.Source.Adapt<CustomerDto>();
                    customer.Id = hit.Id;
                    // 处理高亮
                    if (hit.Highlight != null)
                    {
                        if (hit.Highlight.ContainsKey("desc") && hit.Highlight["desc"].Any())
                        {
                            customer.Desc = string.Join(" ", hit.Highlight["desc"]);
                        }
                        if (hit.Highlight.ContainsKey("hobby") && hit.Highlight["hobby"].Any())
                        {
                            customer.Hobby =new List<string> { string.Join(",", hit.Highlight["hobby"]) };
                        }
                    }
                    // 处理inner_hits
                    if (hit.InnerHits != null && hit.InnerHits.ContainsKey("order"))
                    {
                        var orderHits = hit.InnerHits["order"].Hits.Hits;
                        // 处理订单数据...
                    }
                    results.Add(customer);
                }
            }
            else
            {
                // 处理错误
                Console.WriteLine($"查询失败: {searchResponse.DebugInformation}");
            }
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> test()
        {
            var ret2 = await _elasticClient.SearchAsync<CustomerDoc>(s => 
            s.Query(q => q.MatchAll(_ => { })).Size(0)
            .Aggregations(aggregations =>
             aggregations.Add("agg_hobby", 
                aggregation => aggregation.Terms(t=>t.Field(f=>f.Hobby.Suffix("keyword")).Size(10).Order(new Dictionary<Field, SortOrder> { { "_count", SortOrder.Asc } }))
            )
            // 订单金额聚合 - 使用字符串字段名
            .Add("order_amounts", aggregation =>
                // 订单金额聚合 - 避免嵌套Aggregations调用
                 aggregation.Children(c=>c.Type("order"))
                   .Aggregations(a=>a.Add("agg_orderamount_range",
                   b=>b.Range(r=>r.Field("order_amount")
                    .Ranges(f=>f.To(1000), f => f.From(1000).To(2000), f => f.From(2000))).Aggregations(a=>
                    a.Add("agg_orderamount_stat",b =>b.Stats(st => st
                        .Field("order_amount")
                    )))
                   )
                   //.Add("agg_orderamount_stat",b =>b.Stats(st => st
                   //     .Field("order_amount")
                   // ))
                   ))
            .Add("email_domain",aggregations=>
                aggregations.Terms(t=>t.Field(f=>f.Email).Script(s => s.Source("doc['email'].value.substring(doc['email'].value.indexOf('@') + 1)")).Size(20))
                )
            ));

            var aggDict = new Dictionary<string, HashSet<string>>();
            var hobbyAggs = ret2.Aggregations?.GetStringTerms("agg_hobby")!;
            var hobbyList = new HashSet<string>();
            foreach (var item in hobbyAggs.Buckets)
            {
                hobbyList.Add(item.Key.Value.ToString());
            }
            aggDict.Add("hobby",hobbyList);

            var emailAggs = ret2.Aggregations?.GetStringTerms("email_domain")!;
            var emailList = new HashSet<string>();
            foreach (var item in emailAggs.Buckets)
            {
                emailList.Add(item.Key.Value.ToString());
            }
            aggDict.Add("email_domain", emailList);



            //await _elasticClient.BulkAsync();



            //var add6 =  await _elasticClient.IndexAsync(new Customer
            // {
            //     Id = "6",
            //     Email = "66@qq.com",
            //     Name = "test6",
            //     Desc = "欢迎666莅临指导",
            //     Hobby = new List<string> {"爬山","上网" },
            //     CustomerOrder = JoinField.Root<Customer>()
            // });

            // var get6 = await _elasticClient.GetAsync<Customer>("6");

            // var update6 = await _elasticClient.UpdateAsync<Customer, dynamic>("6", doc => doc.Doc(new
            // {
            //     email="6_1@qq.com",
            //     customer_order = JoinField.Root<Customer>()
            // }).DocAsUpsert());

            //get6.Source.Email = "6_2@qq.com";
            //var update62 = await _elasticClient.UpdateAsync<Customer,Customer>("6",u=>u.Doc(get6.Source));

            //var delete6 = await _elasticClient.DeleteAsync<Customer>("6");


            //var ret = await _elasticClient.IndexAsync(new Customer()
            //{
            //   Id="6",
            //   Name="������",
            //   Email="443@qq.com",
            //   Desc="",
            //   Hobby=new List<string> { "����","��Զ","����"}
            //});

            //await _elasticClient.DeleteByQueryAsync<Customer>(q => q.MatchAll());

            //var ret = await _elasticClient.IndexAsync(new Order { 
            //     Id = "oid_12",
            //     CustomerId="6",
            //     OrderAmount=12,
            //     OrderId= "12",
            //     OrderSerial= "ORD20230012",
            //     OrderTime=DateTime.Now,
            //     CustomerOrder=  JoinField.Link("order", "6")
            //},i=>i.Routing("6"));

            // 创建索引映射
            var createIndexRequest = new CreateIndexRequest("test")
            {
                Mappings = new TypeMapping
                {
                    Properties = new Properties
                    {
                        // customer_order 字段 (join 类型，定义父子关系)
                        ["customer_order"] = new JoinProperty
                        {
                            EagerGlobalOrdinals = true,
                            Relations = new Dictionary<string, Union<string, ICollection<string>>>
                            {
                                { "customer", new[] { "order" } }
                            }
                        },

                        // desc 字段 (text，使用中文分词器)
                        ["desc"] = new TextProperty
                        {
                            Analyzer = "ik_max_word",
                            SearchAnalyzer = "ik_smart"
                        },

                        // email 字段 (keyword)
                        ["email"] = new KeywordProperty(),

                        // hobby 字段 (text + keyword，使用中文分词器)
                        ["hobby"] = new TextProperty
                        {
                            Fields = new Properties
                            {
                                ["keyword"] = new KeywordProperty()
                            },
                            Analyzer = "ik_max_word",
                            SearchAnalyzer = "ik_smart"
                        },

                        // id 字段 (keyword)
                        ["id"] = new KeywordProperty(),

                        // name 字段 (keyword)
                        ["name"] = new KeywordProperty(),

                        // order_amount 字段 (double)
                        ["order_amount"] = new DoubleNumberProperty(),

                        // order_id 字段 (keyword)
                        ["order_id"] = new KeywordProperty(),

                        // order_serial 字段 (keyword)
                        ["order_serial"] = new KeywordProperty(),

                        // order_time 字段 (date)
                        ["order_time"] = new DateProperty()
                    }
                },
                Settings = new IndexSettings
                {
                    NumberOfShards = 1,
                    NumberOfReplicas = 1,
                    Analysis = new IndexSettingsAnalysis
                    {
                        // 如果需要自定义分析器，可以在这里添加
                    }
                }
            };
            var ret = await _elasticClient.Indices.CreateAsync(createIndexRequest);

            return Ok(ret.IsValidResponse);
        }
    }
}
