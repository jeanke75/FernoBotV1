using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FernoBotV1.Services;

namespace FernoBotV1.Modules.Gambling
{
    public class GambleModule
    {
        [Command("dice")]
        [Summary("Give someone a big hug.")]
        public Task Dice(IUserMessage msg) => DiceRoll(msg, 1, 6);

        [Command("dice")]
        [Summary("Give someone a big hug.")]
        public Task Dice(IUserMessage msg, int max) => DiceRoll(msg, 1, max);

        [Command("dice")]
        [Summary("Give someone a big hug.")]
        public Task Dice(IUserMessage msg, int min, int max) => DiceRoll(msg, 1, max);

        private async Task DiceRoll(IUserMessage msg, int min, int max)
        {
            var channel = (ITextChannel)msg.Channel;
            try
            {
                if (min > max) throw new ArgumentException("The first argument should be bigger than the second.");
                int rolled = new NadekoRandom().Next(min, max + 1);

                await channel.SendMessageAsync($"{msg.Author.Mention} rolled {rolled}.").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await channel.SendMessageAsync($":anger: {ex.Message}").ConfigureAwait(false);
            }
        }
    }
}
