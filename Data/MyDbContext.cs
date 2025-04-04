using Microsoft.EntityFrameworkCore;
using System.Data;

namespace seachdemo.Data
{
    public class MyDbContext:DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options):base(options)
        {
            
        }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Biz_Order> Biz_Order { get; set; }
    }
}
