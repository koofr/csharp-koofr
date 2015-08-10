using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class Version
    {
        [JsonProperty("version")]
        public string VersionInfo { get; set; }
        public bool CanUpdate { get; set; }
        public bool ShouldUpdate { get; set; }
        public bool Outdated { get; set; }
    }
}