using System;
using Microsoft.EntityFrameworkCore;

namespace TheWall.Models
{
    public class WallContext : DbContext
    {
        public WallContext(DbContextOptions options) : base(options) { }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
    }
}

