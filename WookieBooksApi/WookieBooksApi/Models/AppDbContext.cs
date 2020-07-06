using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WookieBooksApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // We are doing this so that we disable the default behaviour to use Delete Cascade on foreign keys
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.Restrict;

            modelBuilder.Entity<Books>().Property(x => x.BookPublished).HasDefaultValue(true);

            modelBuilder.Entity<UserRoleMapping>().HasKey(urm => new { urm.UserId, urm.RoleId });

            modelBuilder.Entity<Role>().HasData(
                    new Role() { RoleId = 1 , RoleName = "Admin" }, new Role { RoleId = 2, RoleName = "Author"});

            modelBuilder.Entity<Author>().Property(x => x.AuthorPseudonym).HasDefaultValue("Wookie");

            modelBuilder.Entity<UserRoleMapping>().Property(x => x.RoleId).HasDefaultValue(2);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Books> Books { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoleMapping> UserRoleMappings { get; set; }
    }
}
