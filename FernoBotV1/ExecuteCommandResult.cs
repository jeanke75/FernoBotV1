using Discord.Commands;

namespace FernoBotV1
{
    public struct ExecuteCommandResult
    {
        public readonly CommandInfo CommandInfo;
        public readonly IResult Result;

        public ExecuteCommandResult(CommandInfo commandInfo, IResult result)
        {
            this.CommandInfo = commandInfo;
            this.Result = result;
        }
    }
}