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

    
    /// <summary>
    /// 包含订单的客户 DTO
    /// </summary>
    public class CustomerWithOrdersDto : CustomerDto
    {
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
