using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudSaba.Data
{
    public class CloudSabaContext : DbContext
    {
        public CloudSabaContext (DbContextOptions<CloudSabaContext> options)
            : base(options)
        {
        }

        public DbSet<CloudSaba.Models.IceCream> IceCream { get; set; } = default!;

        public DbSet<CloudSaba.Models.CartItem>? CartItem { get; set; }

        public DbSet<CloudSaba.Models.Order>? Order { get; set; }
    }
}
