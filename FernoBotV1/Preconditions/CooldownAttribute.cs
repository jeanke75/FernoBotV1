using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace FernoBotV1.Preconditions
{
    public class CooldownAttribute : PreconditionAttribute
    {
        public TimeSpan CooldownTime { get; }

        public Dictionary<ulong, DateTimeOffset> CoolingDown { get; set; }

        public CooldownAttribute(int hours, int minutes, int seconds)
        {
            CooldownTime = new TimeSpan(hours, minutes, seconds);
            CoolingDown = new Dictionary<ulong, DateTimeOffset>();
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            if (CoolingDown.ContainsKey(context.User.Id))
            {

            }
            CoolingDown.
        }
    }
}
