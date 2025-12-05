using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyTitlePawnCompany.Data.Models;

namespace MyTitlePawnCompany.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerReference> CustomerReferences { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<TitlePawn> TitlePawns { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreUser> StoreUsers { get; set; }
        public DbSet<ApprovalPolicy> ApprovalPolicies { get; set; }
        public DbSet<InterestRateTier> InterestRateTiers { get; set; }
        public DbSet<StateSpecialRule> StateSpecialRules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            // Suppress the pending model changes warning during development
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Customer>()
                .HasOne(c => c.Company)
                .WithMany(co => co.Customers)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerReference>()
                .HasOne(cr => cr.Customer)
                .WithMany(c => c.References)
                .HasForeignKey(cr => cr.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TitlePawn>()
                .HasOne(tp => tp.Vehicle)
                .WithMany(v => v.TitlePawns)
                .HasForeignKey(tp => tp.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TitlePawn>()
                .HasOne(tp => tp.Company)
                .WithMany()
                .HasForeignKey(tp => tp.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasOne(p => p.TitlePawn)
                .WithMany(tp => tp.Payments)
                .HasForeignKey(p => p.TitlePawnId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Payment>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Vendor>()
                .HasOne(v => v.Company)
                .WithMany(c => c.Vendors)
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Report>()
                .HasOne(r => r.Company)
                .WithMany()
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Store>()
                .HasOne(s => s.Company)
                .WithMany(c => c.Stores)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoreUser>()
                .HasOne(us => us.User)
                .WithMany(u => u.StoreUsers)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoreUser>()
                .HasOne(us => us.Store)
                .WithMany(s => s.StoreUsers)
                .HasForeignKey(us => us.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TitlePawn>()
                .HasOne(tp => tp.Store)
                .WithMany(s => s.TitlePawns)
                .HasForeignKey(tp => tp.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApprovalPolicy>()
                .HasOne(ap => ap.User)
                .WithOne()
                .HasForeignKey<ApprovalPolicy>(ap => ap.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApprovalPolicy>()
                .HasOne(ap => ap.Company)
                .WithMany()
                .HasForeignKey(ap => ap.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InterestRateTier>()
                .HasOne(irt => irt.Store)
                .WithMany()
                .HasForeignKey(irt => irt.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StateSpecialRule>()
                .HasOne(ssr => ssr.Store)
                .WithMany()
                .HasForeignKey(ssr => ssr.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Note: Query filters for multi-tenancy are applied at runtime in services
            // Do not apply them here to avoid model mismatch with migrations
        }
    }
}
