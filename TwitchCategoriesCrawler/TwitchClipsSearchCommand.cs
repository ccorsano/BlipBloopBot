using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchCategoriesCrawler
{
    [Command("clips", Description = "Crawl Twitch categories and resolve localization from Steam")]
    [HelpOption]
    public class TwitchClipsSearchCommand
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly TwitchApplicationOptions _options;
        private readonly ILogger _logger;

        [Option("-c", CommandOptionType.SingleValue, Description = "Channel name")]
        public string ChannelName { get; set; }

        [Option("--start", CommandOptionType.SingleOrNoValue, Description = "Start time to filter")]
        public DateTime? StartTime { get; set; }

        [Option("--end", CommandOptionType.SingleOrNoValue, Description = "End time to filter")]
        public DateTime? EndTime { get; set; }

        public TwitchClipsSearchCommand(
            TwitchAPIClient twitchApiClient,
            IGDBClient igdbClient,
            IOptions<TwitchApplicationOptions> options,
            ILogger<TwitchCategoriesCrawlerCommand> logger)
        {
            _twitchAPIClient = twitchApiClient;
            _options = options.Value;
            _logger = logger;
        }


        public async Task OnExecute()
        {
            var channels = await _twitchAPIClient.SearchChannelsAsync(ChannelName);
            var channel = channels.First(c => c.BroadcasterLogin == ChannelName);

            var allVideos = new List<HelixClip>();
            await foreach(var video in _twitchAPIClient.EnumerateTwitchChannelClipsAsync(channel.Id, StartTime, EndTime))
            {
                allVideos.Add(video);
                _logger.LogInformation("Clip start: {createdAt}, {url}", video.CreatedAt, video.Url);
            }

            _logger.LogInformation("{clipCount} clips found", allVideos.Count);
            allVideos = allVideos.OrderBy(c => c.CreatedAt).ToList();

            foreach(var video in allVideos)
            {
                Console.WriteLine($"{video.CreatedAt.ToString("yyyy-MM-dd'T'HH:mm:ssZ")}: {video.Url}");
            }

            //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(allVideos, new System.Text.Json.JsonSerializerOptions
            //{
            //    WriteIndented = true
            //}));
        }
    }
}
