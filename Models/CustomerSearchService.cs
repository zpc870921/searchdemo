using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace seachdemo.Models
{
    public class SearchModel
    {
        public string DescContent { get; set; }
        public List<string> Hobbies { get; set; }
        public double? MinOrderAmount { get; set; }
        public double? MaxOrderAmount { get; set; }
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
    }

    public class CustomerWithOrders : Customer
    {
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class CustomerSearchService
    {
        private readonly ElasticsearchClient _client;

        public CustomerSearchService(ElasticsearchClient client)
        {
            this._client = client;
        }


        public async Task<SearchResponse<Customer>> SearchCustomersWithQueryContainer2Async(SearchModel model)
        {
            // 创建 QueryContainer 集合
            var queries = new List<Query>();

            // 添加基本查询条件：文档类型为 customer
            queries.Add(new TermQuery("customer_order") { Value = "customer" });

            // 添加描述内容匹配条件
            if (!string.IsNullOrEmpty(model.DescContent))
            {
                queries.Add(new MatchQuery("desc") { Query = model.DescContent });
            }

            // 添加爱好匹配条件
            if (model.Hobbies != null && model.Hobbies.Count > 0)
            {
                queries.Add(new TermsQuery()
                {
                    Field = "hobby",
                    Terms = new TermsQueryField(model.Hobbies.Select(h => (FieldValue)h).ToList())
                });
            }

            // 添加订单金额范围条件
            if (model.MinOrderAmount.HasValue || model.MaxOrderAmount.HasValue)
            {
                var rangeQuery = new NumberRangeQuery("order_amount");

                if (model.MinOrderAmount.HasValue)
                    rangeQuery.Gte = model.MinOrderAmount.Value;

                if (model.MaxOrderAmount.HasValue)
                    rangeQuery.Lte = model.MaxOrderAmount.Value;

                var hasChildQuery = new HasChildQuery()
                {
                    Type = "order",
                    Query = rangeQuery,
                    InnerHits = new InnerHits
                    {
                        Size = 5,
                        Sort = new List<SortOptions>
                {
                    SortOptions.Field("order_amount", new FieldSort(){ Order = SortOrder.Desc })
                }
                    }
                };

                queries.Add(hasChildQuery);
            }

            // 创建 BoolQuery 并设置 Must 条件
            var boolQuery = new BoolQuery
            {
                Must = queries
            };

            // 构建请求
            var request = new SearchRequest("customerorder")
            {
                From = model.From,
                Size = model.Size,
                Query = boolQuery,
                Sort = new List<SortOptions>
        {
            SortOptions.Field("id", new FieldSort(){ Order = SortOrder.Desc })
        },
                Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
            {
                { new Field("desc"), new HighlightField() },
                { new Field("hobby"), new HighlightField() }
            },
                    PreTags = new[] { "<em>" },
                    PostTags = new[] { "</em>" }
                }
            };

            // 执行查询
            var response = await _client.SearchAsync<Customer>(request);

            // 处理 ID 映射
            foreach (var hit in response.Hits)
            {
                if (hit.Source != null)
                {
                    hit.Source.Id = hit.Id;
                }
            }

            return response;
        }

        public async Task<SearchResponse<Customer>> SearchCustomersWithFunctionalQueryContainer3Async(SearchModel model)
        {
            // 使用函数式 API 构建 QueryContainer
            Query finalQuery = null;

            // 添加基本查询条件
            finalQuery = new TermQuery("customer_order") { Value = "customer" };

            // 添加描述内容匹配条件
            if (!string.IsNullOrEmpty(model.DescContent))
            {
                Query descQuery = new MatchQuery("desc") { Query = model.DescContent };
                finalQuery = finalQuery && descQuery; // 使用 && 操作符组合查询
            }

            // 添加爱好匹配条件
            if (model.Hobbies != null && model.Hobbies.Count > 0)
            {
                Query hobbiesQuery = new TermsQuery()
                {
                    Field = "hobby",
                    Terms = new TermsQueryField(model.Hobbies.Select(h => (FieldValue)h).ToList())
                };
                finalQuery = finalQuery && hobbiesQuery;
            }

            // 添加订单金额范围条件
            if (model.MinOrderAmount.HasValue || model.MaxOrderAmount.HasValue)
            {
                var rangeQuery = new NumberRangeQuery("order_amount");

                if (model.MinOrderAmount.HasValue)
                    rangeQuery.Gte = model.MinOrderAmount.Value;

                if (model.MaxOrderAmount.HasValue)
                    rangeQuery.Lte = model.MaxOrderAmount.Value;

                var hasChildQuery = new HasChildQuery()
                {
                    Type = "order",
                    Query = rangeQuery,
                    InnerHits = new InnerHits
                    {
                        Size = 5,
                        Sort = new List<SortOptions>
                {
                    SortOptions.Field("order_amount", new FieldSort(){ Order = SortOrder.Desc })
                }
                    }
                };

                finalQuery = finalQuery && hasChildQuery;
            }

            // 构建请求
            var request = new SearchRequest("customerorder")
            {
                From = model.From,
                Size = model.Size,
                Query = new BoolQuery { Must = new List<Query> { finalQuery } },
                Sort = new List<SortOptions>
                {
                    SortOptions.Field("id", new FieldSort(){ Order = SortOrder.Desc })
                },
                Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
                    {
                        { new Field("desc"), new HighlightField() },
                        { new Field("hobby"), new HighlightField() }
                    },
                    PreTags = new[] { "<em>" },
                    PostTags = new[] { "</em>" }
                }
            };

            // 执行查询
            var response = await _client.SearchAsync<Customer>(request);

            // 处理 ID 映射
            foreach (var hit in response.Hits)
            {
                if (hit.Source != null)
                {
                    hit.Source.Id = hit.Id;
                }
            }

            return response;
        }

        //public async Task<SearchResponse<Customer>> SearchWithQueryContainerDescriptorAsync(SearchModel model)
        //{
        //    var response = await _client.SearchAsync<Customer>(s => s
        //        .Index("customerorder")
        //        .From(model.From)
        //        .Size(model.Size)
        //        .Sort(sort => sort.Descending("id"))
        //        .Query(q => BuildQuery(q, model))
        //        .Highlight(h => h
        //            .Fields(
        //                f => f.Field("desc"),
        //                f => f.Field("hobby")
        //            )
        //            .PreTags("<em>")
        //            .PostTags("</em>")
        //        )
        //    );

        //    // 处理 ID 映射
        //    foreach (var hit in response.Hits)
        //    {
        //        if (hit.Source != null)
        //        {
        //            hit.Source.Id = hit.Id;
        //        }
        //    }

        //    return response;
        //}

        //// 构建查询的辅助方法
        //private QueryContainer BuildQuery(QueryContainerDescriptor<Customer> queryContainerDescriptor, SearchModel model)
        //{
        //    // 创建一个 Bool 查询
        //    return queryContainerDescriptor.Bool(b => {
        //        // 基本查询条件
        //        b.Must(m => m.Term(t => t.Field("customer_order").Value("customer")));

        //        // 根据条件添加更多查询
        //        if (!string.IsNullOrEmpty(model.DescContent))
        //        {
        //            b.Must(m => m.Match(mt => mt.Field("desc").Query(model.DescContent)));
        //        }

        //        if (model.Hobbies != null && model.Hobbies.Count > 0)
        //        {
        //            b.Must(m => m.Terms(t => t.Field("hobby").Terms(model.Hobbies.Cast<object>())));
        //        }

        //        if (model.MinOrderAmount.HasValue || model.MaxOrderAmount.HasValue)
        //        {
        //            b.Must(m => m.HasChild<Order>(hc => hc
        //                .Type("order")
        //                .Query(q => q
        //                    .Range(r => {
        //                        var range = r.Field("order_amount");
        //                        if (model.MinOrderAmount.HasValue)
        //                            range.GreaterThanOrEquals(model.MinOrderAmount.Value);
        //                        if (model.MaxOrderAmount.HasValue)
        //                            range.LessThanOrEquals(model.MaxOrderAmount.Value);
        //                        return range;
        //                    })
        //                )
        //                .InnerHits(ih => ih
        //                    .Size(5)
        //                    .Sort(s => s.Descending("order_amount"))
        //                )
        //            ));
        //        }

        //        return b;
        //    });
        //}

        public async Task<SearchResponse<Customer>> SearchCustomersWithConditionsAsync(SearchModel model)
        {
            // 创建查询对象 - 使用 must 和 filter 分离评分和非评分条件
            var boolQuery = new BoolQuery()
            {
                Must = new List<Query>(),     // 用于评分的条件（相关性查询）
                Filter = new List<Query>()    // 用于过滤的条件（不影响评分）
            };

            // 添加必须条件：文档类型为customer（不影响评分，移到filter中）
            boolQuery.Filter.Add(new TermQuery(Infer.Field<Customer>(c=>c.CustomerOrder)) { Value = "customer" });

            // 添加描述内容匹配条件（这是相关性查询，保留在must中）
            if (!string.IsNullOrEmpty(model.DescContent))
            {
                boolQuery.Must.Add(new MatchQuery(Infer.Field<Customer>(c=>c.Desc)) { Query = model.DescContent });
            }

            // 添加爱好匹配条件（取决于使用场景）
            // 如果需要精确匹配爱好列表，应该放在filter中
            // 如果需要根据爱好相关性评分，应该放在must中
            if (model.Hobbies != null && model.Hobbies.Count > 0)
            {
                var hobbiesQuery = new TermsQuery() 
                { 
                    Field = Infer.Field<Customer>(c=>c.Hobby), 
                    Terms = new TermsQueryField(model.Hobbies.Select(h => (FieldValue)h).ToList()) 
                };
                
                // 这里假设爱好是精确匹配，不需要影响评分
                boolQuery.Filter.Add(hobbiesQuery);
                
                // 如果需要根据爱好进行相关性评分，可以使用以下代码
                // boolQuery.Must.Add(hobbiesQuery);
            }

            // 添加订单金额范围条件（这是过滤条件，不影响评分）
            if (model.MinOrderAmount.HasValue || model.MaxOrderAmount.HasValue)
            {
                var rangeQuery = new NumberRangeQuery( Infer.Field<Order>(o=>o.OrderAmount));

                if (model.MinOrderAmount.HasValue)
                    rangeQuery.Gte = model.MinOrderAmount.Value;

                if (model.MaxOrderAmount.HasValue)
                    rangeQuery.Lte = model.MaxOrderAmount.Value;

                // 使用has_child查询找到有符合条件订单的客户
                var hasChildQuery = new HasChildQuery()
                {
                    Type = "order",
                    Query = rangeQuery,
                    InnerHits = new InnerHits
                    {
                        Size = 5,  // 增加一点，便于获取更多相关订单
                        Sort = new List<SortOptions>
                        {
                            SortOptions.Field(Infer.Field<Order>(o=>o.OrderAmount), new FieldSort(){ Order = SortOrder.Desc })
                        }
                    }
                };

                // 父子查询也是过滤条件，不影响评分
                boolQuery.Filter.Add(hasChildQuery);
            }

            // 如果没有任何相关性条件，添加一个匹配所有文档的查询
            if (boolQuery.Must.Count == 0)
            {
                boolQuery.Must.Add(new MatchAllQuery());
            }

            // 构建查询请求
            var request = new SearchRequest(Infer.Index<Customer>())
            {
                From = model.From,
                Size = model.Size,
                Query = boolQuery,
                Sort = new List<SortOptions>
                {
                    SortOptions.Field("id", new FieldSort(){ Order = SortOrder.Desc })
                },
                SearchType= SearchType.DfsQueryThenFetch,
                Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
                    {
                        { Infer.Field<Customer>(c=>c.Desc), new HighlightField(){
                         Type= HighlighterType.Unified, FragmentSize=150, NumberOfFragments=2 } },
                        { Infer.Field<Customer>(c=>c.Hobby), new HighlightField() }
                    },
                    PreTags = new[] { "<em>" },
                    PostTags = new[] { "</em>" }
                }
            };

            // 执行查询
            var response = await _client.SearchAsync<Customer>(request);
            
            // 确保将 ES 的 _id 映射到 Customer 对象的 Id 属性
            foreach (var hit in response.Hits)
            {
                if (hit.Source != null)
                {
                    hit.Source.Id = hit.Id;
                }
            }
            
            return response;
        }

        // 新增方法：获取客户及其订单信息（包括处理父子关系）
        public async Task<List<CustomerWithOrders>> SearchCustomersWithOrdersAsync(SearchModel model)
        {
            var response = await SearchCustomersWithConditionsAsync(model);
            var result = new List<CustomerWithOrders>();

            foreach (var hit in response.Hits)
            {
                var customerWithOrders = new CustomerWithOrders
                {
                    Id = hit.Id,
                    Name = hit.Source.Name,
                    Email = hit.Source.Email,
                    Hobby = hit.Source.Hobby,
                    Desc = hit.Source.Desc,
                    CustomerOrder = hit.Source.CustomerOrder
                };

                // 处理内部子文档的订单信息
                if (hit.InnerHits != null && hit.InnerHits.Count > 0 && hit.InnerHits.ContainsKey("order"))
                {
                    // 正确处理内部命中的订单数据
                    foreach (var orderHit in hit.InnerHits["order"].Hits.Hits)
                    {
                        if (orderHit.Source != null)
                        {
                            // 从 orderHit.Source 中提取数据并创建一个新的 Order 对象
                            var orderData = orderHit.Source as System.Text.Json.JsonElement?;
                            if (orderData.HasValue)
                            {
                                try
                                {
                                    // 手动构建 Order 对象
                                    var order = new Order
                                    {
                                        Id = orderHit.Id, // 设置为 ES 文档的 _id
                                        CustomerId = GetPropertyValue<string>(orderData.Value, "customer_id"),
                                        OrderId = GetPropertyValue<string>(orderData.Value, "order_id"),
                                        OrderAmount = GetPropertyValue<decimal>(orderData.Value, "order_amount"),
                                        OrderSerial = GetPropertyValue<string>(orderData.Value, "order_serial"),
                                        OrderTime = GetPropertyValue<DateTime>(orderData.Value, "order_time"),
                                        CustomerOrder = "order"
                                    };

                                    // 如果子订单没有父ID，可以在这里设置
                                    if (string.IsNullOrEmpty(order.CustomerId))
                                    {
                                        order.CustomerId = hit.Id;
                                    }

                                    customerWithOrders.Orders.Add(order);
                                }
                                catch (Exception ex)
                                {
                                    // 记录错误但继续处理
                                    Console.WriteLine($"处理订单时出错: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                result.Add(customerWithOrders);
            }

            return result;
        }

        // 辅助方法：从 JsonElement 中获取属性值
        private T GetPropertyValue<T>(System.Text.Json.JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)property.GetString();
                    }
                    else if (typeof(T) == typeof(long))
                    {
                        return (T)(object)property.GetInt64();
                    }
                    else if (typeof(T) == typeof(decimal))
                    {
                        return (T)(object)property.GetDecimal();
                    }
                    else if (typeof(T) == typeof(DateTime))
                    {
                        return (T)(object)property.GetDateTime();
                    }
                    // 添加其他需要的类型转换
                }
                catch
                {
                    // 类型转换失败时，返回默认值
                }
            }
            return default(T);
        }

        //// 使用功能评分查询的高级搜索方法
        //public async Task<SearchResponse<Customer>> SearchCustomersWithFunctionScoreAsync(SearchModel model)
        //{
        //    // 创建基础查询对象
        //    var boolQuery = new BoolQuery()
        //    {
        //        Must = new List<Query>(),
        //        Filter = new List<Query>()
        //    };

        //    // 添加基本过滤条件：文档类型为customer
        //    boolQuery.Filter.Add(new TermQuery("customer_order") { Value = "customer" });

        //    // 添加描述内容匹配条件
        //    if (!string.IsNullOrEmpty(model.DescContent))
        //    {
        //        boolQuery.Must.Add(new MatchQuery("desc") { Query = model.DescContent });
        //    }

        //    // 添加爱好匹配条件
        //    if (model.Hobbies != null && model.Hobbies.Count > 0)
        //    {
        //        boolQuery.Filter.Add(new TermsQuery()
        //        {
        //            Field = "hobby",
        //            Terms = new TermsQueryField(model.Hobbies.Select(h => (FieldValue)h).ToList())
        //        });
        //    }

        //    // 添加订单金额范围条件
        //    if (model.MinOrderAmount.HasValue || model.MaxOrderAmount.HasValue)
        //    {
        //        var rangeQuery = new NumberRangeQuery("order_amount");

        //        if (model.MinOrderAmount.HasValue)
        //            rangeQuery.Gte = model.MinOrderAmount.Value;

        //        if (model.MaxOrderAmount.HasValue)
        //            rangeQuery.Lte = model.MaxOrderAmount.Value;

        //        var hasChildQuery = new HasChildQuery()
        //        {
        //            Type = "order",
        //            Query = rangeQuery,
        //            InnerHits = new InnerHits
        //            {
        //                Size = 5,
        //                Sort = new List<SortOptions>
        //                {
        //                    SortOptions.Field("order_amount", new FieldSort(){ Order = SortOrder.Desc })
        //                }
        //            }
        //        };

        //        boolQuery.Filter.Add(hasChildQuery);
        //    }

        //    // 如果没有任何相关性条件，添加一个匹配所有文档的查询
        //    if (boolQuery.Must.Count == 0)
        //    {
        //        boolQuery.Must.Add(new MatchAllQuery());
        //    }

        //    // 创建功能评分查询
        //    var functionScoreQuery = new FunctionScoreQuery
        //    {
        //        Query = boolQuery,
        //        Functions = new List<FunctionScoreContainer>
        //        {
        //            // 示例1: 基于字段值的评分函数 - 根据爱好的数量增加评分
        //            new FunctionScoreContainer
        //            {
        //                FieldValueFactor = new FieldValueFactorScoreFunction
        //                {
        //                    Field = "hobby.keyword", // 使用 hobby 字段的关键字版本
        //                    Factor = 1.2,        // 乘以这个因子
        //                    Modifier = FieldValueFactorModifier.Log1p, // 应用log(1+x)修饰符，避免极值
        //                    Missing = 1         // 如果字段缺失则使用此值
        //                },
        //                Weight = 2              // 权重
        //            },

        //            // 示例2: 衰减函数 - 根据日期的新旧程度影响评分
        //            // 这个例子假设我们有一个 "created_date" 字段
        //            /* 
        //            new FunctionScoreContainer
        //            {
        //                Gauss = new GaussDateDecayFunction
        //                {
        //                    Field = "created_date",
        //                    Origin = DateTime.Now,
        //                    Scale = new Time("30d"),
        //                    Decay = 0.5
        //                },
        //                Weight = 1
        //            }
        //            */
        //        },
        //        ScoreMode = FunctionScoreMode.Sum,       // 如何组合多个函数的得分
        //        BoostMode = FunctionBoostMode.Multiply,  // 如何将函数得分与查询得分组合
        //        MinScore = 0.1                           // 最小分数，低于此分数的文档将被过滤掉
        //    };

        //    // 构建查询请求
        //    var request = new SearchRequest("customerorder")
        //    {
        //        From = model.From,
        //        Size = model.Size,
        //        Query = functionScoreQuery,
        //        Sort = new List<SortOptions>
        //        {
        //            // 这里我们不使用明确的排序，而是依赖功能评分查询得到的相关性评分
        //            // 如果仍需使用明确排序，可以取消下面的注释
        //            // SortOptions.Field("id", new FieldSort(){ Order = SortOrder.Desc })
        //        },
        //        TrackScores = true, // 确保返回分数
        //        Highlight = new Highlight
        //        {
        //            Fields = new Dictionary<Field, HighlightField>
        //            {
        //                { new Field("desc"), new HighlightField() },
        //                { new Field("hobby"), new HighlightField() }
        //            },
        //            PreTags = new[] { "<em>" },
        //            PostTags = new[] { "</em>" }
        //        }
        //    };

        //    // 执行查询
        //    var response = await _client.SearchAsync<Customer>(request);
            
        //    // 确保将 ES 的 _id 映射到 Customer 对象的 Id 属性
        //    foreach (var hit in response.Hits)
        //    {
        //        if (hit.Source != null)
        //        {
        //            hit.Source.Id = hit.Id;
        //        }
        //    }
            
        //    return response;
        //}
    }
}
