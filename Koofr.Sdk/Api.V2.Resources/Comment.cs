namespace Koofr.Sdk.Api.V2.Resources
{
    public class Comment
    {
        public string Id { get; set; }
        public User User { get; set; }
        public string Content { get; set; }
        public long Added { get; set; }
    }
}