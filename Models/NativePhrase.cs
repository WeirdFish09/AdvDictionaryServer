using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class NativePhrase : IComparable<NativePhrase>
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Phrase {get; set;}
        
        public int CompareTo(NativePhrase other)
        {
            return Phrase.CompareTo(other.Phrase);
        }
    }    
}