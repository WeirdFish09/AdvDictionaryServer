using System;

namespace AdvDictionaryServer.Models
{
    public class WordPrioritiesJSON
    {
        public NativePhraseJson Phrase{get; set;}
        public ForeignWordJSON Word{get; set;}
        public LanguageJSON Language{get ;set;}
        public int Value {get; set;}
        
    }    
}