using System;

namespace AdvDictionaryServer.Models
{
    public class NativePhraseJson : IEquatable<NativePhraseJson>
    {
        public string Phrase {get; set;}

        public bool Equals(NativePhraseJson other)
        {
            return Phrase == other.Phrase;
        }
    }    
}