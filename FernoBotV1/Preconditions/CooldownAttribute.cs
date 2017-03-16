using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System.Collections.Concurrent;

namespace FernoBotV1.Preconditions
{
    public class CooldownAttribute : PreconditionAttribute
    {
        public TimeSpan CooldownTime { get; }

        private ConcurrentDictionary<ulong, DateTimeOffset> CoolingDown { get; set; }

        public CooldownAttribute(int hours, int minutes, int seconds)
        {
            CooldownTime = new TimeSpan(hours, minutes, seconds);
            CoolingDown = new ConcurrentDictionary<ulong, DateTimeOffset>();
            //optional extra todo: start timer for regular cleanup
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (CoolingDown.ContainsKey(context.User.Id))
            {
                DateTimeOffset endTime;
                if (CoolingDown.TryGetValue(context.User.Id, out endTime))
                {
                    if (endTime > DateTimeOffset.Now)
                    {
                        string  name = (context as Discord.IGuildUser)?.Nickname ?? context.User.Username;                       
                        await context.Channel.SendMessageAsync($"{name}, {Math.Round((endTime - DateTimeOffset.Now).TotalSeconds, 0)} seconds before you can use this command again.");
                        return PreconditionResult.FromError("not cooled down yet");
                    }
                }
                else
                {
                    //something really glitchy happened, timer interfered?
                    //for now we'll ignore this possibility except for logging it
                    Console.WriteLine("glitchy cooldown error occurred.");
                }
            }
            CoolingDown.AddOrUpdate(context.User.Id, DateTimeOffset.Now + CooldownTime, (key, val) => DateTimeOffset.Now + CooldownTime);
            return PreconditionResult.FromSuccess();
        }
    }
}
