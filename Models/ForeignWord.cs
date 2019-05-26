using System;
using System.ComponentModel.DataAnnotations;

namespace AdvDictionaryServer.Models
{
    public class ForeignWord : IComparable<ForeignWord>
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Word {get; set;}

        public int CompareTo(ForeignWord other)
        {
            return Word.CompareTo(other.Word);
        }
    }    
}