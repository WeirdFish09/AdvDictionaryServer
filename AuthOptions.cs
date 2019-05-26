using Microsoft.IdentityModel.Tokens;
using System.Text;
 
namespace AdvDictionaryServer
{
    public class AuthOptions
    {
        public const string ISSUER = "AdvDictionaryServer";
        public const string AUDIENCE = "http://0.0.0.0:5000/";
        const string KEY = "super secure mega key for this weird encryption with some spaces";
        public const int LIFETIME = 300;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}