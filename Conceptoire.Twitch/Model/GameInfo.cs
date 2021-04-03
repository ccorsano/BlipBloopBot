namespace Conceptoire.Twitch.Model
{
    public class GameInfo
    {
        public string TwitchCategoryId { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public string Summary { get; set; }
        public string Source { get; set; }
        public ulong? IGDBId { get; set; }
    }
}
