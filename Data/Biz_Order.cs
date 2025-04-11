namespace seachdemo.Data
{
    public class Biz_Order
    {
        public string Id { get; set; }

        public string CustomerId { get; set; }

        public string OrderId { get; set; }

        public decimal OrderAmount { get; set; }

        public string OrderSerial { get; set; }

        public DateTime OrderTime { get; set; }

        public Customer Customer { get; set; }
    }

    public class Biz_Order2
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int Status { get; set; }
        public decimal Amount { get; set; }

        public string SerialCode { get; set; }

        public DateTime C_time { get; set; }

        public Customer Customer { get; set; }
    }
}
