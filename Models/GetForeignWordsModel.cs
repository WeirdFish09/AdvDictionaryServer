namespace AdvDictionaryServer.Models
{
    public class GetForeignWordsModel
    {
            public int Amount { get; set; }   
            public int Offset { get; set; }

            public string Language{get; set;}
    }
}