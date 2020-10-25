using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FtpudDiscordUI;
using FtpudDiscordUI.Elements;

namespace PekaDiscordBot
{

    internal class Program
    {
        private DiscordSocketClient _client;
        private UiHelper _ui;
        
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            var token = File.ReadAllText("token.txt");
            _client = new DiscordSocketClient();
            _ui = new UiHelper(_client);
            
            _client.Log += ClientOnLog;
            _client.MessageReceived += ClientOnMessageReceived;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }


        private async Task ClientOnMessageReceived(SocketMessage arg)
        {
            Console.WriteLine(arg + " " + arg.Author.Discriminator);
            if (arg.Content.Contains( "init" ))
            {
                RestUserMessage msg = await arg.Channel.SendMessageAsync("Hi " + arg.Channel.Id);
            }
        }

        private bool FirstConnect = true;
        private async Task ClientOnLog(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            if (arg.ToString().Contains("Ready") && FirstConnect)
            {
                FirstConnect = false;
                // console:    769566513631461388
                // main:       769296012585467938
                // triggers:   769706227579617321
                // announces:  769721392094249032
                var channel = _client.GetGuild(769296012585467935).GetTextChannel(769566513631461388);

                // todo uncomment
                await _ui.DisplayPage(new MainMenuPage(_ui), channel);

               TriggerService.TriggerService triggerService = 
                   new TriggerService.TriggerService(_client, 769706227579617321, 769721392094249032, 769296012585467935);
               await triggerService.Initizalize();
            }
        }

        
    }
}


// Emoji unicodes:
// https://apps.timwhitlock.info/emoji/tables/unicode
// https://www.fileformat.info/info/emoji/list.htm