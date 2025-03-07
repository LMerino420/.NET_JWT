using JWT_NET9.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWT_NET9.Data
{
    public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }
}
