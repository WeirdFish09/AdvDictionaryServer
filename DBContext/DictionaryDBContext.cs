using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AdvDictionaryServer.Models;

namespace AdvDictionaryServer.DBContext
{
    public class DictionaryDBContext : IdentityDbContext<User>
    {
        public DictionaryDBContext(DbContextOptions options) : base(options)
        {
            
        }
        public DbSet<Language> Languages {get; set;}
        public DbSet<NativePhrase> NativePhrases {get; set;}
        public DbSet<ForeignWord> ForeignWords {get; set;}
        public DbSet<WordPriority> WordPriorities {get; set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Language>().Property(l => l.ID).ValueGeneratedOnAdd();
            builder.Entity<NativePhrase>().Property(p => p.ID).ValueGeneratedOnAdd();
            builder.Entity<ForeignWord>().Property(w => w.ID).ValueGeneratedOnAdd();
            builder.Entity<WordPriority>().Property(p => p.ID).ValueGeneratedOnAdd();
        }

    }
}