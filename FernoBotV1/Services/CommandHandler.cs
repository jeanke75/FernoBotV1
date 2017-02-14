using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace FernoBotV1.Services
{
    public class CommandHandler
    {
        private CommandService commandService;
        private DiscordShardedClient client;
        private List<IDMChannel> ownerChannels { get; set; }
        public ConcurrentDictionary<ulong, uint> UserMessagesSent { get; } = new ConcurrentDictionary<ulong, uint>();
        private IDependencyMap map;

        public class IGuildUserComparer : IEqualityComparer<IGuildUser>
        {
            public bool Equals(IGuildUser x, IGuildUser y) => x.Id == y.Id;

            public int GetHashCode(IGuildUser obj) => obj.Id.GetHashCode();
        }

        public CommandHandler(DiscordShardedClient _client, CommandService _commands)
        {
            // Create Command Service, inject it into Dependency Map
            client = _client;
            commandService = _commands;
            map = new DependencyMap();
            map.Add(_commands);

            //await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            //client.MessageReceived += HandleCommand;
        }

        /*public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            // Mark where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message has a valid prefix, adjust argPos 
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos))) return;

            // Create a Command Context
            var context = new CommandContext(client, message);
            // Execute the Command, store the result
            var result = await commands.ExecuteAsync(context, argPos, map);

            // If the command failed, notify the user
            if (!result.IsSuccess)
                await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
        }*/

        public async Task StartHandling()
        {
            ownerChannels = (await Task.WhenAll(client.Guilds.SelectMany(g => g.Users)
                                  .Where(u => u.Id == 140470317440040960)
                                  .Distinct(new IGuildUserComparer())
                                  .Select(async u => { try { return await u.CreateDMChannelAsync(); } catch { return null; } })))
                                      .Where(ch => ch != null)
                                      .ToList();

            if (!ownerChannels.Any())
                await Console.Out.WriteLineAsync("No owner channels created! Make sure you've specified correct OwnerId in the credentials.json file.");
            else
                await Console.Out.WriteLineAsync($"Created {ownerChannels.Count} out of {1} owner message channels.");

            client.MessageReceived += MessageReceivedHandler;
        }

        const float oneThousandth = 1.0f / 1000;

        private async Task LogSuccessfulExecution(SocketUserMessage usrMsg, SocketTextChannel channel, int ticks)
        {
            await Console.Out.WriteLineAsync(string.Format("Command Executed after {4}s\n\t" +
                        "User: {0}\n\t" +
                        "Server: {1}\n\t" +
                        "Channel: {2}\n\t" +
                        "Message: {3}",
                        usrMsg.Author + " [" + usrMsg.Author.Id + "]", // {0}
                        (channel == null ? "PRIVATE" : channel.Guild.Name + " [" + channel.Guild.Id + "]"), // {1}
                        (channel == null ? "PRIVATE" : channel.Name + " [" + channel.Id + "]"), // {2}
                        usrMsg.Content,
                        ticks * oneThousandth));
        }

        private void LogErroredExecution(SocketUserMessage usrMsg, ExecuteCommandResult exec, SocketTextChannel channel, int ticks)
        {
            Console.Out.WriteLine(string.Format("Command Errored after {5}s\n\t" +
                        "User: {0}\n\t" +
                        "Server: {1}\n\t" +
                        "Channel: {2}\n\t" +
                        "Message: {3}\n\t" +
                        "Error: {4}",
                        usrMsg.Author + " [" + usrMsg.Author.Id + "]", // {0}
                        (channel == null ? "PRIVATE" : channel.Guild.Name + " [" + channel.Guild.Id + "]"), // {1}
                        (channel == null ? "PRIVATE" : channel.Name + " [" + channel.Id + "]"), // {2}
                        usrMsg.Content,// {3}
                        exec.Result.ErrorReason, // {4}
                        ticks * oneThousandth // {5}
                        ));
        }

        private async Task MessageReceivedHandler(SocketMessage msg)
        {
            try
            {
                if (msg.Author.IsBot || !FernoBot.Ready) //no bots, wait until bot connected and initialized
                    return;

                var execTime = Environment.TickCount;

                var usrMsg = msg as SocketUserMessage;
                if (usrMsg == null) //has to be an user message, not system/other messages.
                    return;

                // track how many messagges each user is sending
                UserMessagesSent.AddOrUpdate(usrMsg.Author.Id, 1, (key, old) => ++old);

                var channel = msg.Channel as SocketTextChannel;
                var guild = channel?.Guild;

                string messageContent = usrMsg.Content;

                //--------------------------------------------------
                // Mark where the prefix ends and the command begins
                int argPos = 0;
                // Determine if the message has a valid prefix, adjust argPos 
                if (!(usrMsg.HasMentionPrefix(client.CurrentUser, ref argPos) || usrMsg.HasCharPrefix(FernoBot.botPrefix, ref argPos))) return;
                messageContent = messageContent.Substring(argPos);
                //--------------------------------------------------

                // execute the command and measure the time it took
                var exec = await ExecuteCommand(new CommandContext(client, usrMsg), messageContent, map, MultiMatchHandling.Best);
                execTime = Environment.TickCount - execTime;

                if (exec.Result.IsSuccess)
                {
                    await LogSuccessfulExecution(usrMsg, channel, execTime).ConfigureAwait(false);
                }
                else if (!exec.Result.IsSuccess && exec.Result.Error != CommandError.UnknownCommand)
                {
                    LogErroredExecution(usrMsg, exec, channel, execTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CommandHandler");
                Console.WriteLine(ex);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception of the error in CommandHandler");
                    Console.WriteLine(ex.InnerException);
                }
            }
        }

        public Task<ExecuteCommandResult> ExecuteCommandAsync(CommandContext context, int argPos, IDependencyMap dependencyMap = null, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
            => ExecuteCommand(context, context.Message.Content.Substring(argPos), dependencyMap, multiMatchHandling);


        public async Task<ExecuteCommandResult> ExecuteCommand(CommandContext context, string input, IDependencyMap dependencyMap = null, MultiMatchHandling multiMatchHandling = MultiMatchHandling.Exception)
        {
            dependencyMap = dependencyMap ?? DependencyMap.Empty;

            var searchResult = commandService.Search(context, input);
            if (!searchResult.IsSuccess)
                return new ExecuteCommandResult(null, searchResult);

            var commands = searchResult.Commands;
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                var preconditionResult = await commands[i].CheckPreconditionsAsync(context).ConfigureAwait(false);
                if (!preconditionResult.IsSuccess)
                {
                    if (commands.Count == 1)
                        return new ExecuteCommandResult(null, preconditionResult);
                    else
                        continue;
                }

                var parseResult = await commands[i].ParseAsync(context, searchResult, preconditionResult).ConfigureAwait(false);
                if (!parseResult.IsSuccess)
                {
                    if (parseResult.Error == CommandError.MultipleMatches)
                    {
                        TypeReaderValue[] argList, paramList;
                        switch (multiMatchHandling)
                        {
                            case MultiMatchHandling.Best:
                                argList = parseResult.ArgValues.Select(x => x.Values.OrderByDescending(y => y.Score).First()).ToArray();
                                paramList = parseResult.ParamValues.Select(x => x.Values.OrderByDescending(y => y.Score).First()).ToArray();
                                parseResult = ParseResult.FromSuccess(argList, paramList);
                                break;
                        }
                    }

                    if (!parseResult.IsSuccess)
                    {
                        if (commands.Count == 1)
                            return new ExecuteCommandResult(null, parseResult);
                        else
                            continue;
                    }
                }

                var cmd = commands[i].Command;

                return new ExecuteCommandResult(cmd, await commands[i].ExecuteAsync(context, parseResult, dependencyMap));
            }

            return new ExecuteCommandResult(null, SearchResult.FromError(CommandError.UnknownCommand, "This input does not match any overload."));
        }
    }
}