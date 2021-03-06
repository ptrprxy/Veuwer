﻿using System.Data.Entity;

namespace Veuwer.Models
{
    public class DefaultContext : DbContext
    {
        public DefaultContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<ImageLink> ImageLinks { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<PageView> PageViews { get; set; }
    }
}