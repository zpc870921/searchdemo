using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using seachdemo.Data;

namespace seachdemo.EntityConfigs
{
    public class OrderConfig : IEntityTypeConfiguration<Biz_Order2>
    {
        public void Configure(EntityTypeBuilder<Biz_Order2> builder)
        {
            builder.Property(o=>o.CustomerId).HasColumnName("customer_id");
            builder.Property(o=>o.SerialCode).HasColumnName("serial_code");
            builder.HasOne(o=>o.Customer).WithMany(c=>c.Biz_Orders).HasForeignKey(o=>o.CustomerId).IsRequired();
        }
    }
}
