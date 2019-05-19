using System;

namespace AdvDictionaryServer.Models
{
    public class WordPrioritiesJSON : IEquatable<WordPrioritiesJSON>
    {
        public NativePhraseJson Phrase{get; set;}
        public ForeignWordJSON Word{get; set;}
        public LanguageJSON Language{get ;set;}
        public int Value {get; set;}

        public bool Equals(WordPrioritiesJSON other)
        {
            return Value == other.Value &
                   Phrase.Equals(other.Phrase) &
                   Word.Equals(other.Word) &
                   Language.Equals(other.Language);
        }
    }    
}