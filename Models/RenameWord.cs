using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvDictionaryServer.Models
{
    public class RenameWord
    {
        public string Language { get; set; }
        public string OriginalWord { get; set; }
        public string NewWord { get; set; }
    }
}
