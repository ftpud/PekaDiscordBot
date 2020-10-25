using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace PekaDiscordBot.TriggerService
{
    public class TriggerService
    {
        private DiscordSocketClient _client;
        private ulong _triggersChannelId;
        private SocketTextChannel _announceChannel;
        private ulong _guildId;

        private Emoji subscribeEmoji = new Emoji("\u2795");

        private SocketGuild _guild;

        private List<String> triggersList = new List<String>();

        private Timer songTimer;
        
        private const string songInfoUri = "http://192.168.0.129:8080/wapi/current";
        
        public TriggerService(DiscordSocketClient client, ulong triggersChannelId, ulong announceChannelId, ulong guild)
        {
            _client = client;
            _triggersChannelId = triggersChannelId;
            _guildId = guild;
            _guild = _client.GetGuild(_guildId);
            _client.ReactionAdded += ClientOnReactionAdded;
            _client.ReactionRemoved += ClientOnReactionRemoved;
            _client.MessageReceived += ClientOnMessageReceived;
            _announceChannel = _guild.GetTextChannel(announceChannelId);
        }

        private bool Initialized = false;
        public async Task Initizalize()
        {
            if (Initialized) return;
            Console.WriteLine("TriggerService initialize...");
            var channel = _client.GetGuild(_guildId).GetTextChannel(_triggersChannelId);
            //var messages = channel.GetCachedMessages();
            //int i = 0;
            
            var messages = channel.GetMessagesAsync().Flatten();
            await messages.ForEachAsync(async m => await InitializeTrigger(m));
            songTimer = new Timer();
            songTimer.Interval = 5000;
            songTimer.Elapsed += SongTimerOnElapsed;
            songTimer.Start();
            Initialized = true;
        }

        private string lastAnnounceMsg = "";
        private string lastResponse = "";
        private bool notifierIsInProgress = false;
        private async void SongTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (notifierIsInProgress)
            {
                return;
            }

            notifierIsInProgress = true;
            try
            {
                string song = await GetUriAsync(songInfoUri);
                SongResponseDto resp = JsonConvert.DeserializeObject<SongResponseDto>(song);

                
                string title = resp.CurrentSong.Split('/').Last();
                if (title.Contains("&title="))
                {
                    title=title.Split(new String[] {"&title="}, StringSplitOptions.None).Last();
                }
                
                if (lastResponse != title)
                {
                    await _client.SetGameAsync(title, "https://www.twitch.tv/ascalot1", ActivityType.Streaming);
                    lastResponse = title;
                }

                string mentionString = "";
                triggersList.ForEach(async itm =>
                {
                    if (resp.CurrentSong.ToLower().Contains(itm.ToLower()) || resp.Tags.ToLower().Contains(itm.ToLower()))
                    {
                        var role = _guild.Roles.FirstOrDefault(role => role.Name == itm);
                        if (role != null)
                        {
                            mentionString += MentionUtils.MentionRole(role.Id) + " ";
                        }
                    }
                });

                if (mentionString != "")
                {
                    string msg = $"{mentionString} на канале играет: {title}";
                    if (msg != lastAnnounceMsg)
                    {
                        lastAnnounceMsg = msg;
                        await _announceChannel.SendMessageAsync(msg);
                    }
                }
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            notifierIsInProgress = false;
        }

        private async Task InitializeTrigger(IMessage message)
        {
            if (!message.Content.StartsWith("`"))
            {
                await message.AddReactionAsync(subscribeEmoji);
                await UpdateMessageRoles(message);
            }
        }

        private async Task UpdateMessageRoles(IMessage message)
        {
            try
            {
                var roles = _guild.Roles;

                IRole role = roles.FirstOrDefault(r => r.Name == message.Content);
                if (role == null)
                {
                    role = await _guild.CreateRoleAsync(message.Content, GuildPermissions.None, Color.Default, false,
                        true);
                }

                var reactions = message.GetReactionUsersAsync(subscribeEmoji, 100).Flatten();
                await reactions.ForEachAsync(async r =>
                    await _guild.GetUser(r.Id).AddRoleAsync(role)
                );
                triggersList.Add(message.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task ClientOnMessageReceived(SocketMessage arg)
        {
            if ( arg.Channel.Id == _triggersChannelId && arg.Author.Id != _client.CurrentUser.Id)
            {
                await InitializeTrigger(arg);
            }
        }

        private async Task ClientOnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                if (arg3.UserId != _client.CurrentUser.Id && arg3.Channel.Id == _triggersChannelId && arg3.Emote.Name == subscribeEmoji.Name)
                {
                    var msg = await arg1.DownloadAsync();
                    var roles = _guild.Roles;
                    IRole role = roles.FirstOrDefault(r => r.Name == msg.Content);
                    await _guild.GetUser(arg3.UserId).RemoveRoleAsync(role);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                if (arg3.UserId != _client.CurrentUser.Id && arg3.Channel.Id == _triggersChannelId &&
                    arg3.Emote.Name == subscribeEmoji.Name)
                {
                    var msg = await arg1.DownloadAsync();
                    IRole role = _guild.Roles.FirstOrDefault(r => r.Name == msg.Content);
                    await _guild.GetUser(arg3.UserId).AddRoleAsync(role);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public async Task<string> GetUriAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
    
     
    public class SongResponseDto    {
        public string CurrentSong { get; set; } 
        public string Tags { get; set; } 
    }
}