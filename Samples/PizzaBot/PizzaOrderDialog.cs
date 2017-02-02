using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotTranslator.Bot;
using BotTranslator.Dialog;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using BotTranslator;
using BotTranslator.Services;

namespace Microsoft.Bot.Sample.PizzaBot
{
    [LuisModel("4311ccf1-5ed1-44fe-9f10-a6adbad05c14", "6d0966209c6e4f6b835ce34492f3e6d9", LuisApiVersion.V2)]
    [Serializable]
    class PizzaOrderDialog : TranslatingLuisDialog
    {

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            //check the language - it may not be english!

            if (TranslatorService.Instance.GetLanguage(context) != "en")
            {
                var checkLanguage = await TranslatorService.Instance.Detect(result.Query);
                if (checkLanguage != "en")
                {
                    context.UserData.SetValue("checkLanguage", checkLanguage);

                    EditablePromptDialog.Choice(context,
                           LanuageSelectionChoicesAsync,
                           new List<string> { "Yes", "No" },
                           await TranslatorService.Instance.Translate("You are not speaking English! Would you like me to translate for you?", "en", checkLanguage),
                           await TranslatorService.Instance.Translate("I didn't understand that. Please choose one of the options", "en", checkLanguage),
                           2);

                    return;}
            }

            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        public async Task LanuageSelectionChoicesAsync(IDialogContext context, IAwaitable<string> result)
        {
            try
            {

                string choice = await result;

                if (choice.ToLower() == "no")
                {
                    await context.PostAsync("No troubles, ignore me.");
                }
                else
                {
                    string checkLanguage;
                    context.UserData.TryGetValue<string>("checkLanguage", out checkLanguage);
                    if (string.IsNullOrWhiteSpace(checkLanguage))
                    {
                        await context.PostAsync("Something went wrong and I could not detect the language.");
                    }
                    else
                    {
                        TranslatorService.Instance.SetLanguage(context, checkLanguage);
                        await context.PostAsync("No problem - it has been set!");
                    }
                }
            }
            catch (TooManyAttemptsException tme)
            {
                await context.PostAsync("Sorry, I wasn't able to understand your response. Please try asking for session information again.");
                context.Wait(MessageReceived);
            }
            catch (Exception e)
            {
                await context.PostAsync("An error ocurred within TimeSlotChoiceAsync, please try again later.");
                context.Wait(MessageReceived);

            }
        }

        [LuisIntent("OrderPizza")]
        [LuisIntent("UseCoupon")]
        public async Task ProcessPizzaForm(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            if (!entities.Any((entity) => entity.Type == "Kind"))
            {
                // Infer kind
                foreach (var entity in result.Entities)
                {
                    string kind = null;
                    switch (entity.Type)
                    {
                        case "Signature": kind = "Signature"; break;
                        case "GourmetDelite": kind = "Gourmet delite"; break;
                        case "Stuffed": kind = "stuffed"; break;
                        default:
                            if (entity.Type.StartsWith("BYO")) kind = "byo";
                            break;
                    }
                    if (kind != null)
                    {
                        entities.Add(new EntityRecommendation(type: "Kind") { Entity = kind });
                        break;
                    }
                }
            }

            var topicChoices = new List<string>
            {
                "Ham",
                "Pineapple",
                "Meatlovers"
            };
          

            EditablePromptDialog.Choice(context,
                        PizzaFormComplete,
                        topicChoices,
                        "Which toppings?",
                        "I didn't understand that. Please choose one of the toppings",
                        2);
        }

        private async Task PizzaFormComplete(IDialogContext context, IAwaitable<string> result)
        {
            string order = null;
            try
            {
                order = await result;
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("You canceled the form!");
                return;
            }

            if (order != null)
            {
                await context.PostAsync("Your Pizza Order: " + order.ToString());
            }
            else
            {
                await context.PostAsync("Form returned empty response!");
            }

            context.Wait(MessageReceived);
        }
    }
}