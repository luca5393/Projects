using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using LifeDiscordBot.Commands;

namespace Discord_bot
{
    class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        static async Task Main(string[] args)
        {
            string token = "MTE4MTEzNzQ5Nzg5NjUzMDA4MQ.Gu23hL.Ppd-yqWSamDDs0ZVcu_P274oIbpSjWmb8FSTkg";
            string prefix = "!";

            var discordconfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordconfig);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += Client_Ready;

            var commandconfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false

            };

            Commands = Client.UseCommandsNext(commandconfig);

            var slashcommandsconfig = Client.UseSlashCommands();

            slashcommandsconfig.SlashCommandErrored += async (s, e) =>
            {
                if (e.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (var check in slex.FailedChecks)
                    {
                        if (check is SlashCooldownAttribute att)
                        {
                            var remainingCooldown = att.GetRemainingCooldown(e.Context);
                            var formattedTime = $"{(int)remainingCooldown.TotalHours:00}:{((int)remainingCooldown.TotalMinutes % 60):00}:{remainingCooldown.Seconds:00}";

                            var embed = new DiscordEmbedBuilder
                            {
                                Title = "Cooldown",
                                Description = $"Time left until you can use this command: {formattedTime}",
                                Color = DiscordColor.Red
                            };

                            await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                        }
                    }
                }
            };

            slashcommandsconfig.RegisterCommands<maincommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
