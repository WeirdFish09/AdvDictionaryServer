namespace AdvDictionaryServer.Models
{
    public class GetWordPrioritiesModel
    {
        public string Language { get; set; }
        public int Amount { get; set; }
        public int Offset{get; set;}
    }
}