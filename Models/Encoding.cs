using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class Encoding
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string WordEncoding {get; set;}
        
    }    
}