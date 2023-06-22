using System;
using System.Data.Entity;
using System.Linq;

namespace DB_server.DataModel
{
    public class UserContext : DbContext
    {
        public UserContext()
            : base("name=UserContext")
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}