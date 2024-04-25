using Microsoft.EntityFrameworkCore;
using Programmin2_classroom.Client.Data.Entities;
using Programmin2_classroom.GoogleAuth.Data.Entities;

namespace Programmin2_classroom.Client.Data
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
    }
}
