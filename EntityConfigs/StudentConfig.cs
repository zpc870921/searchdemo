using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using seachdemo.Models;

namespace seachdemo.EntityConfigs
{
    public class StudentConfig:IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("T_student");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
            builder.HasMany(s=>s.Teachers).WithMany(t=>t.Students).UsingEntity("T_teacher_students");
        }
    }
}
