using System.Collections.Generic;
using Koofr.Sdk.Api.V2.Resources;
using Koofr.Sdk.Api.V2.Resources.JsonTypeConverters;
using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2
{
    public partial class KoofrClient
    {
        private class UserUpdate
        {
            public string FirstName { private get; set; }
            public string LastName { private get; set; }
        }

        private class UserPasswordUpdate
        {
            public string NewPassword { get; set; }
            public string OldPassword { get; set; }
        }

        private class NameRequest
        {
            public string Name { get; set; }
        }

        private class GroupList
        {
            public List<Group> Groups { get; set; }
        }

        private class AddUserRequest
        {
            public string Email { get; set; }

            [JsonProperty("permissions", Required = Required.Default)]
            [JsonConverter(typeof(PermissionTypeEnumConverter))]
            public Permissions Permissions { get; set; }
        }
        private class DeviceList
        {
            public List<Device> Devices { get; set; }
        }

        private class DeviceCreateRequest
        {
            public string Name { get; set; }
            public string ProviderName { get; set; }
        }
        private class MountList
        {
            public List<Mount> Mounts { get; set; }
        }

        private class CreateMountRequest
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }
        private class FileLink
        {
            public string Link { get; set; }
        }
        private class FileList
        {
            public List<File> Files { get; set; }
        }

        private class DestinationRequest
        {
            public string ToMountId { get; set; }
            public string ToPath { get; set; }
        }

        private class PostCommentRequest
        {
            public string Content { get; set; }
        }

        private class CommentList
        {
            public List<Comment> Comments { get; set; }
        }

        private class PathRequest
        {
            public string Path { get; set; }
        }

        private class LinkList
        {
            public List<Link> Links { get; set; }
        }

        private class HashRequest
        {
            public string Hash { get; set; }
        }

        private class UpdatePermissionsRequest
        {
            [JsonProperty("permissions", Required = Required.Default)]
            [JsonConverter(typeof(PermissionTypeEnumConverter))]
            public Permissions Permissions { get; set; }
        }

        private class BookmarkList
        {
            public List<Bookmark> Bookmarks { get; set; }
        }
    }
}
