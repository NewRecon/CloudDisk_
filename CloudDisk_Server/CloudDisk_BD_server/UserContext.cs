using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDisk_BD_server
{
    internal class UserContext : DbContext
    {
        public UserContext() : base("DbConnect")
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
