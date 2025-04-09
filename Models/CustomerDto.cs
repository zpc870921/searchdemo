using Elastic.Clients.Elasticsearch.IndexManagement;
using Mapster;
using seachdemo.Data;

namespace seachdemo.Models
{
    /// <summary>
    /// 用于返回给前端的客户 DTO
    /// </summary>
    public class CustomerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Hobby { get; set; }
        public string Desc { get; set; }
    }

    public class CustomerDto2//:IMapFrom<Customer>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Hobby { get; set; }
        //public void ConfigureMapping(TypeAdapterConfig config)
        //{
        //    config.NewConfig<Customer, CustomerDto2>()
        //        //.Ignore(dest=>dest.Email)
        //        .Map(dest => dest.Hobby, source => source.Hobby.Split(';',StringSplitOptions.RemoveEmptyEntries));
        //}
    }


    /// <summary>
    /// 包含订单的客户 DTO
    /// </summary>
    public class CustomerWithOrdersDto : CustomerDto
    {
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
