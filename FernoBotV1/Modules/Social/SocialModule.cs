using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace FernoBotV1.Modules.Social
{
    public class SocialModule : ModuleBase
    {
        [Command("hug")]
        [Summary("Give someone a big hug.")]
        public async Task Invite([Summary("The (optional) user to get info for")] IUser user)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{Context.Message.Author.Username} hugs {userInfo.Username}");
        }
    }
}
