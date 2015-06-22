namespace Koofr.Sdk.Api.V2.Resources
{
    public class Device
    {
        public string Id { get; set; }
        public string ApiKey { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public long SpaceTotal { get; set; }
        public long SpaceUsed { get; set; }
        public string Version { get; set; }
        public bool Readonly { get; set; }
    }
}