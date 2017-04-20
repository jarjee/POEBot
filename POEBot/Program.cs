using System;
using System.Configuration;
using System.IO;
using System.Net;
using Discord;
using POEBot.PoeWiki;

namespace POEBot
{
    internal class Program
    {
        private const string ChatToken = "POEBot.Token";

        public static void Main(string[] args)
        {
            var client = new DiscordClient();

            var poeWikiService = new PoeWikiService();

            client.MessageReceived += async (s, e) =>
            {
                try
                {
                    if (!e.Message.IsAuthor && e.Message.Text.StartsWith("/poe"))
                    {
                        var searchTermRaw = e.Message.Text.Substring(4).Trim();
                        var page = await poeWikiService.GetPoePage(searchTermRaw);

                        if (page != null)
                        {
                            var stream = await poeWikiService.GetWikiImage(page.Url);
                            await e.Channel.SendFile($"{page.Name}.png", stream);
                            await e.Channel.SendMessage($"[{page.Url}]");
                        }
                        else
                        {
                            await e.Channel.SendMessage($"I could not find \"{searchTermRaw}\" in The Archives, @{e.Message.User.Name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await e.Channel.SendMessage(
                        $"Bad news exile, your spell {e.Message.Text} failed but fear not! @{e.Message.User.Name}, your failure will be recorded forever.");
                    await System.Console.Error.WriteLineAsync(ex.ToString());
                }
            };

            //Convert our sync method to an async one and block the Main function until the bot disconnects
            client.ExecuteAndWait(async () =>
            {
                await client.Connect(ConfigurationManager.AppSettings[ChatToken], TokenType.Bot);
            });
        }
    }
}