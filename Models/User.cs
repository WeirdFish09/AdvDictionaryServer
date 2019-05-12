using System;
using Microsoft.AspNetCore.Identity;

namespace AdvDictionaryServer.Models
{
    public class User : IdentityUser
    {
        public User() : base()
        {
            
        }
        public string NativeLanguage{get; set;}
    }
}