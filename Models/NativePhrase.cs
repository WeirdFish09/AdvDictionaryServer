using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class NativePhrase
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Phrase {get; set;}
        
        public Encoding Encoding {get; set;}
    }    
}