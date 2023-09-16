using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public static class JsonHelper
    {
        public static dynamic Deserialize(string json) {
            return ToObject(JToken.Parse(json));
        }

        public static dynamic ToObject(JToken token) {
            switch(token.Type) {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue) token).Value;
            }
        }

        public static string NormalizeString(string str)
        {
            return str
                .Replace(" "/*<= nbsp there*/, " ")  //Removing NBSP
                .Replace("’", "'")
                .Replace("« ", @"""") //translating french quotes to standard one
                .Replace(" »", @"""");//ditto
        }
    }
}
