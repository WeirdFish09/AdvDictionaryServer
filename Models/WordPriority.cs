using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class WordPriority
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public NativePhrase Phrase {get; set;}

        public User User{get; set;}

        public ForeignWord ForeignWord{get; set;}
        
        public Encoding Encoding {get; set;}
    }    
}