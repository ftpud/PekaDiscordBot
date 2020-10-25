using System;
using System.Diagnostics;
using Discord;
using FtpudDiscordUI;
using FtpudDiscordUI.Elements;

namespace PekaDiscordBot
{

    public class ConsoleUtil
    {
        public static string ExecuteProcess(string proc, Process prc = null)
        {
            proc = proc.Replace("\"", "\\\"");
            Console.WriteLine("Executing: " + proc);
            var process = prc;
            if (process == null) process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{proc}\""
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            Console.WriteLine("Starting");
            process.Start();
            Console.WriteLine("Waiting for exit");
            //process.WaitForExit();
            Console.WriteLine("Done");
            return "ok";
        }
    }

    class RestartPage : UiPage
    {
        private UiPage _parentPage;
        public RestartPage(UiHelper helper, UiPage parentPage)
        {
            _parentPage = parentPage;
                
            var label = new TextView("Перезапуск");
            var label2 = new TextView("-------------------");
            var labelHeader = new HeaderText("Управление стримом");
            var list = new ListView(
                new string[]
                {
                    "Частичный перезапуск", 
                    "Полный перезапуск",
                    "Назад"
                }
            );
            var button = new SimpleButton("\u2B07",  async (reaction) =>
            {
                list.Index++;
                await UpdateView();
            });
            var button2 = new SimpleButton("\u2B06",  async (reaction) =>
            {
                list.Index--;
                await UpdateView();
            });
            var button3 = new SimpleButton("\uD83C\uDD97",  async (reaction) =>
            {
                //await Root.Channel.SendMessageAsync(list.Value);
                if (list.Index == 2)
                {
                    await helper.ClosePage(this);
                    await helper.DisplayPage(_parentPage, Root.Channel);
                }
                
                if (list.Index == 1)
                {
                    await helper.SwitchPage(this,  new YesNoDialog(
                        "Перезапуск",
                        "Вы точно хотите сделать полный перезапуск?",
                        async () =>
                        {
                            ConsoleUtil.ExecuteProcess("./../hard_restart.sh");
                            await Root.Channel.SendMessageAsync("Полный перезапуск by " + reaction.User.Value.Username);
                        },
                        async () =>
                        {
                            //await Root.Channel.SendMessageAsync("NO");
                        }, 
                        helper,
                        this
                    ));
                }
                
                if (list.Index == 0)
                {
                    await helper.SwitchPage(this,  new YesNoDialog(
                        "Перезапуск",
                        "Вы точно хотите сделать частичный перезапуск?",
                        async () =>
                        {
                            ConsoleUtil.ExecuteProcess("./../soft_restart.sh");
                            await Root.Channel.SendMessageAsync("Частичный перезапуск by " + reaction.User.Value.Username);
                        },
                        async () =>
                        {
                            //await Root.Channel.SendMessageAsync("NO");
                        }, 
                        helper,
                        this
                    ));
                }
            });
            AddElement(label);
            AddElement(label2);
            AddElement(list);
            AddElement(button2);
            AddElement(button);
            AddElement(button3);
            AddElement(labelHeader);
        }
        
        protected override void Decorate(EmbedBuilder embedBuilder)
        {
            embedBuilder.WithColor(250, 100, 100);
        }
    }
}