using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using seachdemo.Models;

namespace seachdemo
{
    public static class ElasticSearchClientServiceCollection
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services)
        {
            //var nodes = new Uri[]
            //{
            //    new Uri("http://localhost:9200")
            //};

            //var pool = new SniffingNodePool(nodes);

            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                            .DefaultIndex("customerorder")
                            .DefaultMappingFor<CustomerDoc>(i => i
                                .IndexName("customerorder")
                                .IdProperty(p => p.Id)
                                .RelationName("customer")
                            )
                             .DefaultMappingFor<OrderDoc>(i => i
                                .IndexName("customerorder")
                                .RelationName("order")
                                .IdProperty(p => p.Id)
                            )
                            .EnableDebugMode()
                            .PrettyJson()
                            .RequestTimeout(TimeSpan.FromMinutes(2));


            var client = new ElasticsearchClient(settings);
            services.AddSingleton(client);
            services.AddTransient<CustomerSearchService>();
            return services;
        }
    }
}
