using Mapster;
using Mapster.Utils;
using seachdemo.Data;
using seachdemo.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace seachdemo.Mappers
{
    public static class MapsterConfig
    {
        public static void ConfigureMapster(this IServiceCollection services)
        {
            // 全局配置
            TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.Exact);
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            //TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
            TypeAdapterConfig.GlobalSettings.When((srcType, destType, mapType) => srcType == destType)
    .Ignore("Id");
            TypeAdapterConfig.GlobalSettings.When((srcType, destType, mapType) => mapType == MapType.Projection)
    .IgnoreAttribute(typeof(NotMappedAttribute));
            TypeAdapterConfig.GlobalSettings.ScanInheritedTypes(Assembly.GetExecutingAssembly());

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

            TypeAdapterConfig<Customer, CustomerDto2>.NewConfig()
                 .Map(dest => dest.Hobby, src => src.Hobby.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(), srccond => !string.IsNullOrWhiteSpace(srccond.Hobby))
                 .IgnoreIf((src, dest) => !string.IsNullOrWhiteSpace(dest.Name), dest => dest.Name).TwoWays();

            // 配置映射前的逻辑
            TypeAdapterConfig<User, UserDto>.NewConfig()
                .BeforeMapping((src, dest) => {
                    // 如果源对象的LastLoginDate为空，设置一个默认值
                    if (src.LastLoginDate == null)
                    {
                        src.LastLoginDate = DateTime.MinValue;
                    }

                    // 预先初始化目标对象的LastUpdated属性
                    dest.LastUpdated = DateTime.Now;

                    Console.WriteLine($"开始映射用户: {src.FirstName} {src.LastName}");
                })
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
                .AfterMapping((src, dest) =>
                {
                    // 基于LastLoginDate确定用户是否活跃
                    dest.IsActive = src.LastLoginDate > DateTime.Now.AddMonths(-1);

                    // 验证映射结果
                    if (string.IsNullOrEmpty(dest.FullName))
                    {
                        dest.FullName = "未命名用户";
                    }

                    Console.WriteLine($"完成用户映射: {dest.FullName}");
                });
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsActive { get; set; }
    }
}