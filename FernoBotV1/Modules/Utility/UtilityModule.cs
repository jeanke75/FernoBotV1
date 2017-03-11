using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FernoBotV1.Modules.Utility
{
    public class UtilityModule : ModuleBase
    {
        [Command(nameof(Ping))]
        [Discord.Commands.Summary("ping the bot")]
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
