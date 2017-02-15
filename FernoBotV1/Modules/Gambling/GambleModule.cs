using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FernoBotV1.Services;

namespace FernoBotV1.Modules.Gambling
{
    public class GambleModule : ModuleBase
    {
        
        [Command("dice")]
        [Summary("Roll a 6 sided dice.")]
        public Task Dice() => DiceRoll(1, 6);

        [Command("dice")]
        [Summary("Roll an n sided dice.")]
        public Task Dice(int max) => DiceRoll(1, max);

        [Command("dice")]
        [Summary("Roll a dice with numbers between <min> and <max>.")]
        public Task Dice(int min, int max) => DiceRoll(min, max);

        private async Task DiceRoll(int min, int max)
        {
            try
            {
                if (min > max) throw new ArgumentException("The first argument should be bigger than the second.");
                int rolled = new NadekoRandom().Next(min, max + 1);

                await ReplyAsync($"{Context.Message.Author.Mention} rolled {rolled}.").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ReplyAsync($":anger: {ex.Message}").ConfigureAwait(false);
            }
        }
    }
}
