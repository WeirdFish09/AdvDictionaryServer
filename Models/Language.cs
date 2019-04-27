using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class Language
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Name {get; set;}
        
    }    
}