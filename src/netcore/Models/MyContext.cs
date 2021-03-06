﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore.Models
{
    public class MyContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Class> Classes { get; set; }
        public DbSet<Student> Students { get; set; }

        public MyContext(DbContextOptions<MyContext> context) : base(context)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Class>().HasMany(x => x.Students)
                .WithOne()
                .HasForeignKey(x => x.ClassId);
        }
    }
}
