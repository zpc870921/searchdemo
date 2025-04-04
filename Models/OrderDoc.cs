using Elastic.Clients.Elasticsearch;
using seachdemo.Data;
using System.Text.Json.Serialization;

namespace seachdemo.Models
{
    // 订单模型
    public class OrderDoc
    {
        public OrderDoc()
        {
            
        }
        public OrderDoc(Biz_Order order)
        {
            this.Id = "oid_" + order.OrderId;
            this.CustomerId=order.CustomerId;
            this.OrderId = order.OrderId;
            this.OrderAmount = order.OrderAmount;
            this.OrderSerial = order.OrderSerial;
            this.OrderTime = DateTime.Now;
        }
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
