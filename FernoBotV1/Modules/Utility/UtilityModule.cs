using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace FernoBotV1.Modules.Utility
{
    public class UtilityModule : ModuleBase
    {
        [Command(nameof(Ping))]
        [Summary("ping the bot")]
        public async Task Ping()
        {
            try
            {
                await ReplyAsync("Pong dadong");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
