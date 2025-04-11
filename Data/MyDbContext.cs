using Microsoft.EntityFrameworkCore;
using seachdemo.Models;
using System.Data;

namespace seachdemo.Data
{
    public class MyDbContext:DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options):base(options)
        {
            
        }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Biz_Order2> Biz_Order { get; set; }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
        }
    }
}
