using AlaElTalab.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ServiceProvider = AlaElTalab.Models.ServiceProvider;

namespace AlaElTalab.Data
{
    /*
    Use IdentityDbContext When:
    You need user registration/login
    You want built-in password recovery
    You need role-based permissions
    */
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>

    {
        /* 
         coordinations to deal with database and data server and your data 
         Query Translation, Database Connection Management
        DbSet<T>	Represents a database table (e.g., DbSet<Customer> Customers)
        SaveChanges()	Saves all tracked changes to the database
        ModelBuilder	Configures how classes map to tables (in OnModelCreating)
        ChangeTracker	Monitors object states (Added/Modified/Deleted/Unchanged)
         */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            /* we dont write the connection string here for security, and in case i want to change anything
            int the database i don't have to change the code (Environment Flexibility,Different Settings 
            per Environment, Use different databases for Development/Production), so we put the configuration
            string in the app settings or in the json file */
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ServiceProvider> ServiceProviders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        /*
         identifies the one to one relationship between Booking model 
         and Rating
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Rating)
                .WithOne(r => r.Booking)
                .HasForeignKey<Rating>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
            .HasOne(r => r.ServiceProvider)
            .WithMany(sp => sp.Ratings)
            .HasForeignKey(r => r.ServiceProviderId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Service>().HasData(
              new Service { ServiceId = 1, Name = "Housekeeping" },
              new Service { ServiceId = 2, Name = "Electrical Services" },
              new Service { ServiceId = 3, Name = "Plumbing" },
              new Service { ServiceId = 4, Name = "Carpentry" }
          );

        }

    }
}