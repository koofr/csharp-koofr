namespace Koofr.Sdk.Api.V2.Resources
{
    public class Link
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Counter { get; set; }
        public string Url { get; set; }
        public string ShortUrl { get; set; }
        public string Hash { get; set; }
        public string Host { get; set; }
        public bool HasPassword { get; set; }
        public string Password { get; set; }
    }
}