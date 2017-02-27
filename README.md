# Bot Translator

This project helps you integrate Microsoft Bing Translator in to your C# Bot Builder projects. 

You write your bot in your language of choice, and leave the translation heavy lifting to Bing Translator. The translator bolts on around your code, with only a couple of small, non-intrusive changes. 

It works by intercepting and wrapping internal [Bot Builder](https://dev.botframework.com/) objects and intercepting certain calls to perform translation before being handed off to the base framework. 

#### Examples

For another more in-depth example, see [this project](https://github.com/jakkaj/SimpleBot).

<img src="https://cloud.githubusercontent.com/assets/5225782/23356060/9cc8d094-fd2c-11e6-9792-0f116d1b1d41.gif" width="800px"/>

#### Install the Nuget 

```
Install-Package SimpleBotTranslator
```

You will have to add the service reference to you *web.config* file manually. 

```xml
<system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name="BasicHttpBinding_LanguageService" />
      </basicHttpBinding>
    </bindings>
    <client>
      <endpoint address="http://api.microsofttranslator.com/V2/soap.svc"
          binding="basicHttpBinding" bindingConfiguration="BasicHttpBinding_LanguageService"
          contract="MicrosoftTranslator.LanguageService" name="BasicHttpBinding_LanguageService" />
    </client>
  </system.serviceModel>
```

### Translator API Key. 

You will need to get a [Translator API Key](https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/TextTranslation). There are free options, but you will need an [Azure account](https://azure.microsoft.com/en-us/free/).

### Integrate

This project works best with the LuisDialog approach to bot building. [Pizza example](https://github.com/Microsoft/BotBuilder/blob/master/CSharp/Samples/PizzaBot/PizzaOrderDialog.cs) and [Ignite Bot example](https://github.com/MSFTAuDX/SimpleBot/blob/master/SimpleIgniteBot/SimpleIgniteBot/LUIS/LuisModel.cs). The ignite bot includes the language stuff too! You can however use this with other approaches (see the but on Unsupported Bot Builder Functionality at the bottom of this document).

The setup starts in your *MessagesController*. Follow [this](https://github.com/MSFTAuDX/BotTranslator/blob/master/Samples/PizzaBot/Controllers/MessagesController.cs) class example. 

**Add the translator key**

```csharp
 TranslatorService.Instance.SetKey("[Your key here]");
 ```
You can now test your translation is working. 

```csharp
var result = await TranslatorService.Instance.Translate("This is a test", "en", "it");
```

The *result* variable should contain some italian text when you run your site.

With that working it's time to start the auto translation integration. 

In your *MessagesController* add the following to automatically translate all requests coming in.

```csharp
await TranslatorService.Instance.TranslateIn(activity, true); 
```

This will automaticaly translate from to the *activated language* to english. If you're native langauge is other than english, you may edit this setting in the source code and rebuild the project. 

### LUIS Dialog

In our examples we use *LuisDialog*. This is a great way to build bots that use the [LUIS](https://www.luis.ai/) [Cognitive Service](https://www.microsoft.com/cognitive-services/en-us).

Follow along [here (simple)](https://github.com/MSFTAuDX/BotTranslator/blob/master/Samples/PizzaBot/PizzaOrderDialog.cs) and [here (less simple but better example)](https://github.com/MSFTAuDX/SimpleBot/blob/master/SimpleIgniteBot/SimpleIgniteBot/LUIS/LuisModel.cs). 

Base your LuisDialog on *TranslatingLuisDialog*

```csharp
[LuisModel("4311ccf1-5ed1-44fe-9f10-a6adbad05c14", "6d0966209c6e4f6b835ce34492f3e6d9", LuisApiVersion.V2)]
[Serializable]
class PizzaOrderDialog : TranslatingLuisDialog
```

Grab your own [LUIS](http://luis.ai) keys [here](https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/LUIS/pricingtier/S0).

*TranslatingLuisDialog* will automatically intercept your outgoing messages and translate them back to the *activated language*

**NOTE** The SimpleIgniteBot example does not use *TranslatingLuisDialog*, it has the same features implemented direcly in the main LUIS Dialog. 

### Activating Text Translation

Before translation will occur, it needs to be activated. 

In our bots if the users query hits the *no intent* LUIS intent we add extra functionality like [http://qnamaker.ai/](http://qnamaker.ai/) and Azure Search. If those do not return any result then we check the user's language. If it's not english then we ask then if they would like automatic tranlation and store the result in their bot state. 

The calls to *QueryQnaMakerAsync* and *QueryAzureSearch* have been left in as an example of feature fall through, but you can remove them (or add them from [here](https://github.com/MSFTAuDX/SimpleBot/blob/master/SimpleIgniteBot/SimpleIgniteBot/LUIS/LuisModel.cs))

```csharp
[LuisIntent("")]
public async Task NoIntent(IDialogContext context, LuisResult result)
{
    
    var sentReply = await QueryQnaMakerAsync(context, result);

    if (sentReply)
    {
        return;
    }

    // Add Azure Search fall-through here if required
    // sentReply = await QueryAzureSearch(contenxt, result);
    //if (sentReply)
    //{
    //    return;
    //}

    if (_translatorService.GetLanguage(context) != "en")
    {
        var checkLanguage = await _translatorService.Detect(result.Query);
        if (checkLanguage != "en")
        {
            context.UserData.SetValue("checkLanguage", checkLanguage);

            EditablePromptDialog.Choice(context,
                LanuageSelectionChoicesAsync,
                new List<string> {"Yes", "No"},
                await _translatorService.Translate(
                    "You are not speaking English! Would you like me to translate for you?", "en",
                    checkLanguage),
                await _translatorService.Translate(
                    "I didn't understand that. Please choose one of the options", "en",
                    checkLanguage),
                2);

            return;
        }
    }

    context.Wait(MessageReceived);
}
```

This will fire a choice back to the user in their own language asking if they would like to start translation. Note that here we're manually translating - this is becasue the system has not been activated in their language so it needs to beforced manually. 

Also note the use if *EditablePromptDialog*. If you use this to ask your uesrs questions rathern than *PromptDialog* it will automatically translate the dialogs for you. 

This *EditablePromptDialog* has a callback to *LanguageSelectionChoicesAsync*.

```csharp
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
```

This is where we handle the user's language choice and set it to bot state. 

### Reverting to default language

The last bit is allowing the user to disable language. In the simple example we achive this by scanning for "commands" in *MessagesController*  before we translate / call the LuisDialog.

```csharp
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
        //continue on with translation and LuisDialog
```

### Unsupported Bot Builder Functionality

Not all functionality of the Bot Builder will work with automatic translation, although manual translation may be an option. 

A notable exception is FormFlow - due to the way it works internally. Some of the reflection / attribute patterns may not work. 

You can manually translate any piece of text - with the notable exception of some of the attribute functionality in form flow. 