using System;
using System.Web.Http;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;
using BotTranslator;
using BotTranslator.Services;

namespace Microsoft.Bot.Sample.PizzaBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
       
        

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            TranslatorService.Instance.SetKey("[Your key here]");
            if (activity != null)
            {

                if (activity.Text != null && activity.Text.ToLowerInvariant().Contains("command language"))
                {
                    var t = activity.Text.ToLowerInvariant().Replace("command language", "");
                    t = t.Trim();

                    await TranslatorService.Instance.SetLanguage(activity, t);

                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply =
                        activity.CreateReply(await TranslatorService.Instance.TranslateBack(activity,
                            "Your language preference has been set!"));
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                else
                {
                    await TranslatorService.Instance.TranslateIn(activity, true);

                    // one of these will have an interface and process it
                    switch (activity.GetActivityType())
                    {
                        case ActivityTypes.Message:
                            await Conversation.SendAsync(activity, () => new PizzaOrderDialog());
                            break;

                        case ActivityTypes.ConversationUpdate:
                        case ActivityTypes.ContactRelationUpdate:
                        case ActivityTypes.Typing:
                        case ActivityTypes.DeleteUserData:
                        default:
                            Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                            break;
                    }
                }


              
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}