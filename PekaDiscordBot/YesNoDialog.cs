using System;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using FtpudDiscordUI;
using FtpudDiscordUI.Elements;

namespace PekaDiscordBot
{
    public class YesNoDialog : UiPage
    {
        public YesNoDialog(string title, string text, Func<Task> yesAction, Func<Task> noAction, UiHelper helper, UiPage parent)
        {
            var labelHeader = new HeaderText(title);
            var label = new TextView(text);
            
            var yesButton = new SimpleButton("\u2705",  async (reaction) =>
            {
                await yesAction();
                await helper.SwitchPage(this, parent);
            });
            var noButton = new SimpleButton("\u274E",  async (reaction) =>
            {
                await noAction();
                await helper.SwitchPage(this, parent);
            });
           
            AddElement(labelHeader);
            AddElement(label);
            
            AddElement(yesButton);
            AddElement(noButton);
            
        }

        protected override void Decorate(EmbedBuilder embedBuilder)
        {
            embedBuilder.WithColor(255, 0, 0);
        }
    }
}