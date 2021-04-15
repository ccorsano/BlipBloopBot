using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public interface IProcessorSettings
    {
        Task WriteAsync();
        Task ReadAsync();
    }
}