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
        public DbSet<Encoding> Encodings {get; set;}
        public DbSet<Language> Languages {get; set;}
        public DbSet<NativePhrase> NativePhrases {get; set;}
        public DbSet<ForeignWord> ForeignWords {get; set;}
        public DbSet<WordPriority> WordPriorities {get; set;}



    }
}