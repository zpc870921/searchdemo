namespace seachdemo.Data
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Hobby { get; set; }
        public string Desc { get; set; }
        public DateTime C_time { get; set; }
        public DateTime Update_time { get; set; }

        public virtual ICollection<Biz_Order2> Biz_Orders { get; set; } = new List<Biz_Order2>();
    }
}
