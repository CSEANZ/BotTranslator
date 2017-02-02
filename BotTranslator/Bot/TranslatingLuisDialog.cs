using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotTranslator.Dialog;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotTranslator.Bot
{
    [Serializable]
    public class TranslatingLuisDialog : LuisDialog<object>, IDialog<object>
    {
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            if (!(context is TranslatingDialogContext))
            {
                context = new TranslatingDialogContext(context);
            }

            await base.MessageReceived(context, item);
        }

        async Task IDialog<object>.StartAsync(IDialogContext context)
        {
            var translatingContext = new TranslatingDialogContext(context);
            await base.StartAsync(translatingContext);
        }
    }
}
