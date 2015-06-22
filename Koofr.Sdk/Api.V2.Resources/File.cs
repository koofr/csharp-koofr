using Koofr.Sdk.Api.V2.Resources.JsonTypeConverters;
using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class File : INamedEntity
    {
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(FileTypeEnumConverter))]
        public FileType Type { get; set; }
        public long Modified { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public Link Receiver { get; set; }
        public Link Link { get; set; }
        public Bookmark Bookmark { get; set; }
        public Mount Mount { get; set; }
    }
}