using System;
//using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using FernoBotV1.Services;

namespace FernoBotV1
{
    public class FernoBot
    {
        public static char botPrefix = '!';
        public static CommandService service { get; private set; }
        public static CommandHandler handler { get; private set; }
        public static DiscordShardedClient Client { get; private set; }
        //public static ConcurrentDictionary<string, string> ModulePrefixes { get; private set; }
        public static bool Ready { get; private set; }
        public async Task RunAsync(params string[] args)
        {
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance,
                AudioMode = AudioMode.Disabled,
                MessageCacheSize = 10,
                LogLevel = LogSeverity.Warning,
                TotalShards = 1,
                ConnectionTimeout = int.MaxValue
            });
            Client.Log += Client_Log;

            //initialize Services
            service = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync
            });
            handler = new CommandHandler(Client, service);

            await Client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["TOKEN"]).ConfigureAwait(false);
            await Client.ConnectAsync().ConfigureAwait(false);

            await Console.Out.WriteLineAsync("Connected");

            await handler.StartHandling().ConfigureAwait(false);

            await service.AddModulesAsync(this.GetType().Assembly).ConfigureAwait(false);

            Ready = true;
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.Source + " | " + arg.Message);
            if (arg.Exception != null)
                Console.WriteLine(arg.Exception);

            return Task.FromResult(0);
        }

        public async Task RunAndBlockAsync(params string[] args)
        {
            await RunAsync(args).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }
    }
}
