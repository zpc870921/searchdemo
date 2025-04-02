using Elastic.Clients.Elasticsearch;
using System.Text.Json.Serialization;

namespace seachdemo.Models
{
    // 统一的文档模型（包含父子关系）
    public class CustomerOrderDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Hobby { get; set; }
        public string Desc { get; set; }
        public string CustomerId { get; set; }
        public long? OrderId { get; set; }
        public decimal? OrderAmount { get; set; }
        public string OrderSerial { get; set; }
        public DateTime? OrderTime { get; set; }

        //[JsonPropertyName("customer_order")]
        //public JoinField CustomerOrder { get; set; }
    }
}
