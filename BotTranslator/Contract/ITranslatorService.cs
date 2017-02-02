using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotTranslator.Contract
{
    public interface ITranslatorService
    {
        bool IsEnabled(IDialogContext context);
        Task<bool> IsEnabled(Activity activity);
        string GetLanguage(IDialogContext context);
        Task<string> GetLanguage(Activity activity);
        Task SetLanguage(Activity activity, string languageCode);
        Task<string> Translate(string text, string sourceLanguage, string destinationLanguage);
        Task<string> Translate(string text, string sourceLanguage);
        Task<string> TranslateBack(Activity activity, string text);
        Task<string> Detect(string text);
        Task TranslateIn(Activity activity, bool onlyIfEnabled);
        void SetKey(string key);
    }
}
