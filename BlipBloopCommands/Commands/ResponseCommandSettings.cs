using Conceptoire.Twitch.IRC;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands
{
    public class ResponseCommandSettings : IProcessorSettings
    {
        public string Message { get; set; }

        public Task ReadAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}