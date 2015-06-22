using System.Collections.Generic;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class ConnectionList
    {
        public List<User> Users { get; set; }
        public List<Group> Groups { get; set; }
    }
}