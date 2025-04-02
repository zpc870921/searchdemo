using Elastic.Clients.Elasticsearch;
using System.Text.Json.Serialization;

namespace seachdemo.Models
{
    // 订单模型
    public class Order
    {
        //[JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("customer_id")]
        public string CustomerId { get; set; }

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("order_amount")]
        public decimal OrderAmount { get; set; }

        [JsonPropertyName("order_serial")]
        public string OrderSerial { get; set; }

        [JsonPropertyName("order_time")]
        public DateTime OrderTime { get; set; }

        // 父子关系字段
        //[JsonPropertyName("customer_order")]
        public JoinField CustomerOrder { get; set; }
    }
}
