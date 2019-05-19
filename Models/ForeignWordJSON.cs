using System;

namespace AdvDictionaryServer.Models
{
    public class ForeignWordJSON : IEquatable<ForeignWordJSON>
    {
        public string Word {get; set;}

        public bool Equals(ForeignWordJSON other)
        {
            return Word == other.Word;
        }
    }    
}