using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotTranslator.Dialog;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotTranslator.Bot
{
    [Serializable]
    public class EditablePromptChoice<T> : PromptDialog.PromptChoice<T>
    {
        public PromptOptions<T> PromptOptions => promptOptions;

        public EditablePromptChoice(IEnumerable<T> options, string prompt, string retry, int attempts, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null) : base(options, prompt, retry, attempts, promptStyle, descriptions)
        {
        }

        public EditablePromptChoice(PromptOptions<T> promptOptions) : base(promptOptions)
        {
        }

        protected override Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            if (!(context is TranslatingDialogContext))
            {
                context = new TranslatingDialogContext(context);
            }

            return base.MessageReceivedAsync(context, message);
        }
    }

    public class EditablePromptDialog : PromptDialog
    {
        public new static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, PromptOptions<T> promptOptions)
        {
            if (!(context is TranslatingDialogContext))
            {
                context = new TranslatingDialogContext(context);
            }
            var child = new EditablePromptChoice<T>(promptOptions);
            context.Call<T>(child, resume);
        }

        public new static void Choice<T>(IDialogContext context, ResumeAfter<T> resume, IEnumerable<T> options, string prompt, string retry = null, int attempts = 3, PromptStyle promptStyle = PromptStyle.Auto, IEnumerable<string> descriptions = null)
        {
            if (!(context is TranslatingDialogContext))
            {
                context = new TranslatingDialogContext(context);
            }
            Choice(context, resume, new PromptOptions<T>(prompt, retry, attempts: attempts, options: options.ToList(), promptStyler: new PromptStyler(promptStyle), descriptions: descriptions?.ToList()));
        }
    }
}