using Mapster;
using seachdemo.Models;

namespace seachdemo.Mappers
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // 配置 Customer -> CustomerDto 映射
            TypeAdapterConfig<CustomerDoc, CustomerDto>
                .NewConfig()
             ;

            // 配置 Order -> OrderDto 映射
            TypeAdapterConfig<OrderDoc, OrderDto>
                .NewConfig();

            // 配置 CustomerWithOrders -> CustomerWithOrdersDto 映射
            TypeAdapterConfig<CustomerWithOrders, CustomerWithOrdersDto>
                .NewConfig();

            // 全局配置
            TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.Exact);
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        }
    }
} 