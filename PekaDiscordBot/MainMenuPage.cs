using System;
using Discord;
using FtpudDiscordUI;
using FtpudDiscordUI.Elements;

namespace PekaDiscordBot
{
    class MainMenuPage : UiPage
    {
        private UiPage _restartPage;
        public MainMenuPage(UiHelper helper)
        {
            _restartPage = new RestartPage(helper, this);
            
            var labelHeader = new HeaderText("Главное меню");
            var label2 = new TextView("-------------------");
            var label = new TextView("Тут написано что-то умное!");
            var list = new ListView(
                new string[]
                {
                    "Управлние стримом", 
                    "Что-нибудь еще"
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
                if (list._index == 0)
                {
                    await helper.ClosePage(this);
                    await helper.DisplayPage(_restartPage, Root.Channel);
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
            embedBuilder.WithColor(100, 100, 255);
        }
    }
}