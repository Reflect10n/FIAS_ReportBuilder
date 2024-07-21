namespace WebApplication6
{
    using Microsoft.EntityFrameworkCore;
    using WebApplication6.Models;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ObjectLevel> ObjectLevels { get; set; }
        public DbSet<ObjectAsAddr> ObjectAsAddrs { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<UpdateDate> UpdateDates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<UpdateDate>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("UpdateDates");
            });

            modelBuilder.Entity<ObjectAsAddr>()
            .HasOne(o => o.Operation)
            .WithMany()
            .HasForeignKey(o => o.OperType_Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
