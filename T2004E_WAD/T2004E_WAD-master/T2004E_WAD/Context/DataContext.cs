﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using T2004E_WAD.Models;
using System.Web;

namespace T2004E_WAD.Context
{
    public class DataContext :DbContext
    {
        public DataContext() : base("T2004E_WAD")
        {
        }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
    }
}