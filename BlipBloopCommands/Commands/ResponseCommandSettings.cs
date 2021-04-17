using Conceptoire.Twitch.Commands;
using Conceptoire.Twitch.IRC;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands
{
    public class ResponseCommandSettings : IProcessorSettings
    {
        public string[] Aliases { get; set; }
        public string Message { get; set; }

        public void LoadFromOptions(CommandOptions options)
        {
            Aliases = options.Aliases.ToArray();
            if (options.Parameters.TryGetValue("Message", out string message))
            {
                Message = message;
            }
        }

        public void SaveToOptions(CommandOptions options)
        {
            options.Aliases = Aliases.ToArray();
            options.Parameters["Message"] = Message;
        }
    }
}