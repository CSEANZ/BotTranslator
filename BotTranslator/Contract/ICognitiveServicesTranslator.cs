using System.Threading.Tasks;

namespace BotTranslator.Services
{
    public interface ICognitiveServicesTranslator
    {
        Task<string> Detect(string subsKey, string detectString);
        Task<string> Translate(string subsKey, string translateString, string sourceLanguage, string destinationLanguage);
    }
}