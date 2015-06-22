using System.Collections.Generic;
using Koofr.Sdk.Api.V2.Resources.JsonTypeConverters;
using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class Mount : INamedEntity, IPermissionsEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(MountTypeEnumConverter))]
        public MountType Type { get; set; }
        public bool Online { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsShared { get; set; }
        public List<User> Users { get; set; }
        public List<Group> Groups { get; set; }

        [JsonProperty("permissions", Required = Required.AllowNull)]
        [JsonConverter(typeof(PermissionTypeEnumConverter))]
        public Permissions Permissions { get; set; }
        public int Version { get; set; }
        public User Owner { get; set; }
        public long SpaceTotal { get; set; }
        public long SpaceUsed { get; set; }
        public Root Root { get; set; }
    }
}