namespace seachdemo.Models
{
    public class OrderDto
    {
        public string Id { get; set; }

        public string CustomerId { get; set; }

        public string OrderId { get; set; }

        public decimal OrderAmount { get; set; }

        public string OrderSerial { get; set; }

        public DateTime OrderTime { get; set; }
    }
}
