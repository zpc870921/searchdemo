using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using seachdemo.Models;

namespace seachdemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ElasticsearchClient _elasticClient;
        private readonly CustomerSearchService _customerSearchService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,ElasticsearchClient elasticClient, CustomerSearchService customerSearchService)
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

        /// <summary>
        /// 使用功能评分查询
        /// </summary>
        [HttpPost("function-score")]
        public async Task<IActionResult> GetCustomersFunctionScore(SearchModel model)
        {
            //var ret = await _customerSearchService.SearchCustomersWithFunctionScoreAsync(model);

            //return Ok(new
            //{
            //    TotalHits = ret.Total,
            //    MaxScore = ret.MaxScore,
            //    ElapsedMs = ret.Took,
            //    Documents = ret.Documents,
            //    Hits = ret.Hits.Select(h => new
            //    {
            //        Id = h.Id,
            //        Score = h.Score,
            //        Source = h.Source,
            //        Highlights = h.Highlight
            //    }).ToList()
            //});
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> test()
        {
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
                            Relations = new Dictionary<string, Union<string,ICollection<string>>>
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
