﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FernoBotV1.Modules.Public
{
    public class PublicModule : ModuleBase
    {
        [Command("invite")]
        [Summary("Returns the OAuth2 Invite URL of the bot")]
        public async Task Invite()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"A user with `MANAGE_SERVER` can invite me to your server here: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>");
        }

        [Command("leave")]
        [Summary("Instructs the bot to leave this Guild.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Leave()
        {
            if (Context.Guild == null) { await ReplyAsync("This command can only be ran in a server."); return; }
            await ReplyAsync("Leaving~");
            await Context.Guild.LeaveAsync();
        }

        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordShardedClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordShardedClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordShardedClient).Guilds.Sum(g => g.Users.Where(x => !x.IsBot).Count())}"
            );
        }

        [Command("embedtest")]
        public async Task EmbedTest()
        {
            //var embed = new Embed()
            var embed = new EmbedBuilder()
                .WithColor(Color.Default)
                .WithTitle("Info")
                .WithCurrentTimestamp()
                .WithDescription("A test of the embed...")
                .WithFooter(efb => efb.WithText("A footer"));
            await Context.Channel.SendMessageAsync("testest", embed: embed.Build());
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"d\d\ h\h\ m\m\ s\s");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}