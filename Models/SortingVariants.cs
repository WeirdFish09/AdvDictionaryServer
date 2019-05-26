using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvDictionaryServer.Models
{
    public enum SortingVariants { Id, NativePhrase, Priority, ForeignWord}
    public static class SortingOrderingCreator
    {
        public static string CreateOrdering(SortingVariants sv)
        {
            
            switch (sv)
            {
                case SortingVariants.ForeignWord:
                    return "ForeignWord.Word";

                case SortingVariants.NativePhrase:
                    return "NativePhrase.Phrase";
                    
                case SortingVariants.Priority:
                    return "Value";
                    
                default:
                    return "ID";
                    
            }
        }
    }
}
