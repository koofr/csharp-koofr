using System.Collections.Generic;
using Koofr.Sdk.Api.V2.Resources.JsonTypeConverters;
using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class Group: IPermissionsEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }

        [JsonProperty("permissions", Required = Required.Default)]
        [JsonConverter(typeof(PermissionTypeEnumConverter))]
        public Permissions Permissions { get; set; }
    }
}