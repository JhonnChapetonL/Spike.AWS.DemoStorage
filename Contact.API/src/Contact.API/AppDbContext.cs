using Microsoft.EntityFrameworkCore;

namespace Contact.API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public virtual DbSet<Models.Contact> Contact { get; set; }
        public virtual DbSet<Models.Employee> Employee { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Contact>().ToTable("Contact").HasKey(x => x.Id);

            modelBuilder.Entity<Models.Employee>().ToTable("employee").HasKey(x => x.Id);
            modelBuilder.Entity<Models.Employee>(entity =>
            {
                entity.Property(e => e.Id).IsRequired().HasColumnName("id");
                entity.Property(e => e.Name).IsRequired().HasColumnName("name");
                entity.Property(e => e.Age).IsRequired().HasColumnName("age");
                entity.Property(e => e.Department).HasColumnName("department");
            });

        }
    }
}
