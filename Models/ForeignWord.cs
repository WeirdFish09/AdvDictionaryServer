using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class ForeignWord
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Word {get; set;}
        
        public Encoding Encoding {get; set;}
        public Language Language { get; set; }
    }    
}