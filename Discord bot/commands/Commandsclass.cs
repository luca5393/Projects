using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Discord_bot.commands
{
    public class Commandsclass : BaseCommandModule
    {
        [Command("hello")]
        public async Task hellocommand(CommandContext ctx)
        {
            var mess = new DiscordEmbedBuilder
            {
                Title = $"Hello {ctx.User.Username}",
                Color = DiscordColor.Gold
            };

            await ctx.Channel.SendMessageAsync(embed: mess);
        }
        [Command("math")]
        public async Task mathcommand(CommandContext ctx, int num1, int num2) 
        { 
            await ctx.Channel.SendMessageAsync($"Result: {num1+num2}");
        }

    }
}
