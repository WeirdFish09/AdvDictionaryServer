using System;

namespace AdvDictionaryServer.Models
{
    public class LanguageJSON : IEquatable<LanguageJSON>
    {
        public string Name {get; set;}

        public bool Equals(LanguageJSON other)
        {
            return Name == other.Name;
        }
    }    
}