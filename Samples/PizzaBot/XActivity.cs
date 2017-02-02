using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.PizzaBot
{
    public class XActivity : Activity, IActivity,
        IConversationUpdateActivity,
        IContactRelationUpdateActivity,
        IMessageActivity,
        ITypingActivity,
        IEndOfConversationActivity,
        IEventActivity,
        IInvokeActivity
    {
        public XActivity()
        {
            
        }
    }
}