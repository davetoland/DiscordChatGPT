# Authentication with ChatGPT
Before you can start interacting with ChatGPT, you'll need to authenticate and authorize your API calls.

Get an API key from OpenAI by signing up for an account and creating a new Secret Key in the [API Keys](https://beta.openai.com/account/api-keys) section of the OpenAI dashboard.

Then store it in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows):

1. Open a terminal window and navigate to the root directory of the project.

2. Run the following command to enable secret storage: 
```csharp
dotnet user-secrets init
```

3. Then, to store your API key as a secret: 
```csharp
dotnet user-secrets set "openai:api-key" "YOUR ACTUAL API KEY"
```

You can use the following command to list all secrets from the terminal:
```csharp
dotnet user-secrets list
```

# Authenticating and configuring a Discord Bot
You'll need a Discord account, [sign up for one here](https://discord.com/) if you need to.
You can use Discord in the browser, or you can download the desktop application.

1. Firstly, in Discord, add a new Server with the plus [+] icon in the side bar.

2. Then go to the [Developer Portal](https://discord.com/developers/applications).

3. Create a new App and make a note of the Application ID and Public Key.

4. Store these in your user-secrets: 
```csharp
dotnet user-secrets set "discord:application-id" "YOUR APPLICATION ID"
dotnet user-secrets set "discord:public-key" "YOUR PUBLIC KEY"
```

5. Go to the Bot section and add a new Bot and uncheck the Public Bot option, then click `Reset Token`

6. Store the generated token in your user-secrets
```csharp
dotnet user-secrets set "discord:bot-token" "YOUR BOT TOKEN"
```

7. Click `Save Changes`

8. Open the `OAuth2` drop down

9. Select `URL Generator` and configure the options as follows:

    - `SCOPES`:
        - _bot_
        - _applications.commands_
    - `BOT PERMISSIONS`:
        - _Manage Webhooks_
        - _Use Slash Commands_

10. Copy the `GENERATED URL` and paste it into your browser

11. Select the Server you created in step 1 and Authorise the app.




# Sending requests to the ChatGPT API for text generation

To generate text using the ChatGPT API, you will need to send an API request which uses the `text-davinci-003` model. We'll do this by requesting a _completion_, which sends a prompt to ChatGPT and receives a response in return.


## Payload

This is a collection of parameters and their values, most are optional two that we must provide are: `model` and `prompt`. 
The model refers to the AI model that will be used on the OpenAI side, throughout this we'll be using the davinci-3 model. You can read more on models [here](https://beta.openai.com/docs/models/gpt-3).

```csharp
// Provide a payload with the prompt
var payload = new Dictionary<string, object>
    {
        { "prompt", "What is the capital of England?" },
        { "model", "text-davinci-003" }
    };
```


## Specifying the length, quality, and style of the generated text

The OpenAI service offers a range of parameters to help you control the length, quality, and style of the generated text. Among the options available are:

 - `max_tokens [1:*]`: This parameter sets the maximum number of tokens, or words, to be included in the response. It's worth noting that this will truncate responses, rather than making them fit into this many words. Also, there are usage limits and quotas, so be sure to check the OpenAI website for the most up-to-date information before setting this value too high.

 - `best_of [1:*]`: This parameter determines the number of completion attempts the service will make server-side before returning the best result. Generally speaking, a larger sample set will yield higher-quality responses, but again, be mindful of usage limits imposed by OpenAI.

 - `frequency_penalty [-2.0:2.0]`: This parameter penalizes repeated words based on their existing frequency. A positive value will make the response less repetitive, potentially at the expense of exact detail. The context of the request should dictate the value applied here, with a balanced approach used if the parameter is left unset.

 - `temperature [0.0:1.0]`: This parameter controls the level of creativity in the model's response. A higher value will encourage more creative responses, while a lower value will result in responses that are closer to the prompt. High temperature responses can be particularly interesting, as they allow the AI to interpret the prompt more loosely.

There are various different parameters you can experiment with, a full list of them can be found [on the OpenAI website](https://beta.openai.com/docs/api-reference/completions/create).

### Controlling the quality level using `best_of`

The `best_of` parameter instructs the AI to generate multiple responses for a given prompt, and return the one with the highest log probability per token. This means that the model will generate multiple responses to the same prompt and the one with the highest probability of being a good response is returned.

One of the main use cases of this parameter is in creative tasks such as brainstorming, content creation or any other task that requires generating multiple options. The "bestof" parameter can be set to a high value, which will generate a large number of different completions, increasing the chances of coming up with something truly unique and innovative.

Another use case is to increase the diversity of the generated text, as when working with language generation models, it is common for the model to generate responses that are very similar to one another, even when using different prompts. By increasing the `best_of` parameter, you directly increase the diversity of the result.

```csharp
var payload = new Dictionary<string, object>
    {
        { "prompt", "Write a short description of a book called Break Dancing With Crocodiles" },
        { "best_of", 1 },
        { "max_tokens", 250 },
        { "model", "text-davinci-003" }
    };
```

### Best Of: 1

> Break Dancing With Crocodiles is an uplifting story about a young girl's dreams and aspirations. Set in a small town in western Canada, the narrative follows the young girl's struggles to break out of the monotony of her provincial life. Through her dealings with a mysterious crocodile, the girl learns to take risks and accept uncertainty, ultimately finding a way to embrace her own potential. With a strong story that promotes resilience and strength of character, Break Dancing With Crocodiles is an inspiring story of self-discovery.

### Best Of: 5

> Break Dancing With Crocodiles is a heartfelt and timely coming-of-age story about a teenage girl, Bess, and her long and exhilarating journey from suburban Britain to the cultural melting pot of Paris. Through self-discovery and with the help of some unlikely companions, Bess eventually finds the courage to redefine herself and to embrace the world around her. With themes of friendship, identity, family, and music, Break Dancing With Crocodiles is an inspiring and heartfelt read.

### Controlling the frequency and presence of words using `frequency_penalty` and `presence_penalty`

The frequency penalty is a tool that can be used to reduce the amount of repetition in the text generated by an AI model. It works by making small adjustments to the probabilities of certain words, phrases or patterns appearing in the generated text, based on how often those words have been used previously. This makes it less likely that the model will repeat the same phrases or patterns over and over again, resulting in more unique and varied text.

The strength of these penalty can be adjusted using a coefficient, which is set to a value between 0.1 and 1 to reduce repetition moderately, or increased to 2 to strongly suppress repetition, but this can affect the quality of the generated text.


```csharp
var payload = new Dictionary<string, object>
    {
        { "prompt", "Generate 5 sentences about: The C# Language" },
        { "frequency_penalty", 0.5 },
        { "max_tokens", 500 },
        { "model", "text-davinci-003"}
    };
```

For this, the generated text is:

>1. C# is a modern, object-oriented programming language that has been embraced by the community as a powerful solution for application development.
>2. C# offers a vast array of innovative features that make it an ideal language for developing web, mobile, and desktop applications.
>3. The syntax of C# is similar to Java, however it offers various additional built-in features that help accelerate application development.
>4. C# is seen as the best choice for software developers who are looking for the most reliable and efficient language to develop their applications in.
>5. Developed by Microsoft in the early 2000s, C# has now become one of the most popular programming languages in the world.


Whereas if the `frequency_penalty` is changed to a value of 1, we get quite a different result:

> 1. C# is a versatile language, with features such as object-oriented programming capabilities, automatic memory management and garbage collection, and generics.
>2. C# is a general-purpose programming language developed by Microsoft and used to create Windows-based applications. 
> 3. The most recent release of the C# language, version 7.0, includes improvements in performance optimizations for developers working on computer vision tasks or creating machine learning solutions for Azure services.
>4. C# provides developers with access to .NET libraries which allow them to quickly create powerful web-based applications or maintain existing ones on any platform of their choice including Windows or Linux platforms.
> 5. By using Visual Studio Code in conjunction with the .NET Core runtime tools for monitoring performance and debugging problems within code written with the C# language , you can rapidly develop applications more effectively than ever before!

As you can see, the frequency penalty of 1 discourages the model from repeating phrases and patterns it has seen before, resulting in more varied and unique text, but also less coherent and not as meaningful.

### Controlling the creativity level using `temperature`

The Temperature parameter is a powerful tool that can be utilized to unleash the full creative potential of the model. When set to a high value, the model is encouraged to generate responses that are more varied and unexpected, pushing the boundaries of what is possible with language generation technology. This opens up a world of possibilities, as it allows for the generation of unique and compelling content that stands out from the competition. 

High temperature settings are ideal for brainstorming, idea generation and content creation, where creativity and originality are highly valued. It allows the model to explore new and uncharted territories and come up with truly unique and innovative responses.

While a high degree of temperature can lead to varied and creative responses, low temperature responses have its own set of benefits. The model's responses will be more consistent and predictable, which can be useful for tasks that require a high degree of accuracy and precision. For example, in customer service or technical support, low temperature settings provide consistent and accurate information that is easy for customers to understand. Additionally, in scenarios where the goal is to produce language that is more human sounding, low temperature settings make the model's output less distinguishable from a response a human might give. 

```csharp
var payload = new Dictionary<string, object>
    {
        { "prompt", "Write a short description of a white horse called Murphy" },
        { "temperature", 0.0 },
        { "max_tokens", 1500 },
        {"model", "text-davinci-003"}
    };
```

Experiment by adjusting the temperature between values of 0.0 and 1.0 to get some interesting and varied results.

Here are some examples of responses ChatGPT has provided:

### Temperature: 1.0

> Murphy is a beautiful white horse with a gentle and friendly personality. He loves to be around people and is always eager to please. He is a great companion and loves to go on long rides. He is a great jumper and loves to show off his skills. He is a loyal and loving horse who loves to be around his family.

### Temperature: 0.5

> 
> Murphy is a white horse with a strong and muscular build. He is an incredibly gentle and friendly animal, always willing to please his owners. He loves to go for long rides and explore new places, and is an excellent jumper. Murphy is an excellent companion, always eager to learn new things and have fun.

### Temperature: 0.0

> Murphy is a beautiful white horse belonging to a local family. He is quite tall and his coat is clean and shiny. He is friendly and loves attention, and can often be seen happily trotting around the nearby fields. He loves long walks and enjoys being around people, and would make a wonderful companion for anyone looking for a loyal equestrian friend.

As you can see, the responses get more *heated* and intensify in emotion, embellishing the response with more character.


# The Discord Bot
The chatbot is designed to enhance communication within Discord by responding to incoming web connections in real-time. Upon receiving a connection, the bot will listen for a Slash Command, acknowledging the command and parsing the payload to retrieve the text entered by the user. This input is then sent to the ChatGPT API to generate a response, which is subsequently posted in the Discord channel via a webhook call.

## Integrating the chatbot with Discord

Discord provides a powerful API that allows you to create rich interaction between bots and users, most importantly it gives us a free out-of-the-box interface as the frontend of our application. 

However, in order to communicate through Discord we'll need to implement some data structures and models on our side to map between our C# code and the payloads that Discord expects and returns to us.

Here I'm just using anonymous types and dynamic objects to represent the Discord models, the minimum requirement to get an end to end test going. 

For further detail on what each of part of each model represents I recommend you read the documentation, at least in part, around [Application Commands](https://discord.com/developers/docs/interactions/application-commands) (in particular [Slash Commands](https://discord.com/developers/docs/interactions/application-commands#slash-commands) which we'll be using) and [Receiving and Responding to Interactions](https://discord.com/developers/docs/interactions/receiving-and-responding) in our application.

Here I just register a new Slash Command with Discord, but there's much more on offer.


## Tunelling to localhost with _ngrok_

Ultimately you'd host the bot with a cloud provider, but for now we can test locally by using a service like [ngrok](https://ngrok.com).
You can sign up for free, and there's plenty of information on the website about how it works, but in short it will create a secure tunnel through the internet to your development machine.

Once you've signed up for an account and downloaded the relevant software, launch ngrok and run the following command:
```
ngrok http 3000
```

This will output a block of text, in which you'll find your forwarding URL (in this case https://some.random.generated.string.ngrok.io):
```cmd
Forwarding https://some.random.generated.string.ngrok.io -> http://localhost:3000 
```

Head back over to your Application on [Discord](https://discord.com/developers/applications).

1. Go to `General Information`
2. Paste the forwarding URL into the `INTERACTIONS ENDPOINT URL` text box (don't click Save yet!)
3. Make sure you've started the bot and that it's listening
4. Now go back to `General Information` and click `Save Changes`

At this point you should receive the callback from Discord, and as long as everything worked ok the page will allow the save to happen. 

If you still see the `Save Changes` button on screen then you might just have to wait a few minutes. If it persists, something went wrong.
You can view the ngrok logs in the Web Interface which, combined with stepping through the debugger, usually helps to resolve any issues. The ngrok Web Interface URL will be displayed in the console, it'll look something like:

```cmd
Web Interface      http://127.0.0.1:4040  
```

Once the ping from Discord has succeeded it won't ask again, however note that everytime you restart ngrok and get a new Forwaring URL you'll have to repeat the process of registering it on Discord and acknowledging the ping.



# Connecting it all together

A handler for the Slash Command callback fires when a user interacts with the bot. We extract the user's input text from the callback's payload, and then send this to ChatGPT for completion. This could take some time though, so in the meantime we'll send a response to Discord to acknowledge the callback and prevent it marking us as timed out and to say we'll update it soon. Discord automatically displays a "_\<Bot Name\> is thinking..._" animation to the user while it waits, which is quite cool.

When ChatGPT responds, we then post an update to a Discord webhook, which replaces the animation with a copy of the question, and ChatGPT's response.


# Finally...

Go to Discord and try out your Slash Command in the channel you invited your bot to:

![Slash command](https://github.com/davetoland/DiscordChatGPT/assets/19878260/90c4ea15-0447-4630-91cf-94143c7f0ce2 "Slash command")

![Bot is thinking](https://github.com/davetoland/DiscordChatGPT/assets/19878260/921de8a0-bcdf-4aab-96a4-3087db926480 "Bot is thinking")

![Response from ChatGPT](https://github.com/davetoland/DiscordChatGPT/assets/19878260/c5c54231-c459-4511-9df1-5c6db08bab15 "Response from ChatGPT")
