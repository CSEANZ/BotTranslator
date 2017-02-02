using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using BotTranslator.Contract;
using BotTranslator.Glue;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotTranslator.Services
{
    public class TranslatorService : ITranslatorService
    {
        private readonly ICognitiveServicesTranslator _translator;
        protected string _key = null;
        public TranslatorService(ICognitiveServicesTranslator translator)
        {
            _translator = translator;
        }

        public void SetKey(string key)
        {
            _key = key;
        }

        public static ITranslatorService Instance => TranslatorGlue.Container.Resolve<ITranslatorService>();



        public bool IsEnabled(IDialogContext context)
        {
            return !string.IsNullOrWhiteSpace(GetLanguage(context));
        }

        public async Task<bool> IsEnabled(Activity activity)
        {
            return !string.IsNullOrWhiteSpace(await GetLanguage(activity));
        }

        public string GetLanguage(IDialogContext context)
        {
            context.UserData.TryGetValue<string>("LanguageCode", out string code); ;
            return code;
        }

        public async Task<string> GetLanguage(Activity activity)
        {
            var sc = activity.GetStateClient();
            var data = await sc.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            var code = data.GetProperty<string>("LanguageCode");
            return code;
        }

        public async Task SetLanguage(Activity activity, string languageCode)
        {
            var sc = activity.GetStateClient();
            var data = await sc.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            if (string.IsNullOrWhiteSpace(languageCode))
            {
                data.RemoveProperty("LanguageCode");
            }
            else
            {
                data.SetProperty("LanguageCode", languageCode);
            }

            await sc.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, data);
        }



        private string _subsKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_key))
                {
                    throw new ArgumentException("Please call SetKey on TranslatorService first");
                }

                return _key;
            }
        }

        public async Task TranslateIn(Activity activity, bool onlyIfEnabled)
        {
            if (!await IsEnabled(activity))
            {
                return;
            }

            activity.Text = await Translate(activity.Text, await GetLanguage(activity));
        }
        

        public async Task<string> Translate(string text, string sourceLanguage, string destinationLanguage)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var languageResult = await _translator.Translate(_subsKey, text, sourceLanguage, destinationLanguage);

            return languageResult;
        }

        public async Task<string> Translate(string text, string sourceLanguage)
        {
            return await Translate(text, sourceLanguage, "en");
        }

        public async Task<string> TranslateBack(Activity activity, string text)
        {
            var targetLanguage = await GetLanguage(activity);

            if (string.IsNullOrWhiteSpace(targetLanguage))
            {
                return text;
            }

            return await Translate(text, "en", targetLanguage);
        }

        public async Task<string> Detect(string text)
        {
            var languageResult = await _translator.Detect(_subsKey, text);

            return languageResult;
        }
    }
}
