using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace FernoBotV1.Modules.Social
{
    public class SocialModule : ModuleBase
    {
        [Command("hug")]
        [Summary("Give someone a big hug.")]
        public async Task Invite([Summary("The user to hug")] IUser user)
        {
            if (user.Id != Context.Message.Author.Id) {
                await ReplyAsync($"{Context.Message.Author.Username} hugs {user.Username}");
            }
        }
    }
}
