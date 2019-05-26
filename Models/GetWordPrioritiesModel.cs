using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AdvDictionaryServer.Models
{
    public class GetWordPrioritiesModel
    {
        public string Language { get; set; }
        public int Amount { get; set; }
        public int Offset{get; set;}

        [JsonConverter(typeof(StringEnumConverter))]
        public SortingVariants SortingVariant { get; set; }
    }
}