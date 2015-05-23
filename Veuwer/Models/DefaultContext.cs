using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Veuw.Models
{
    public class DefaultContext : DbContext
    {
        public DefaultContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<ImageLink> ImageLinks { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}