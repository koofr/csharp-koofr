using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Koofr.Sdk.Api.V2.Resources;
using Koofr.Sdk.Api.V2.Transfer;
using Koofr.Sdk.Api.V2.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using File = Koofr.Sdk.Api.V2.Resources.File;

namespace Koofr.Sdk.Api.V2
{
    public partial class KoofrClient
    {
        public const string HeaderKoofrUsername = "X-Koofr-Email";
        public const string HeaderKoofrPassword = "X-Koofr-Password";
        public const string HeaderKoofrToken = "X-Koofr-Token";

        private readonly string _baseUrl;
        private readonly HttpClient _client;

        public KoofrClient(String baseUrl, HttpClient client)
        {
            _baseUrl = baseUrl;
            _client = client;
        }

        public string AuthToken { private get; set; }

        public async Task<bool> Authenticate(string username, string password)
        {
            try
            {
                ClearHeaders(_client);
                _client.DefaultRequestHeaders.Add(HeaderKoofrUsername, username);
                _client.DefaultRequestHeaders.Add(HeaderKoofrPassword, password);

                HttpResponseMessage response = await _client.GetAsync(_baseUrl + "/token");
                if (response.IsSuccessStatusCode && response.Headers != null &&
                    response.Headers.Contains(HeaderKoofrToken))
                {
                    AuthToken = response.Headers.GetValues(HeaderKoofrToken).FirstOrDefault();

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }

            return false;
        }

        private void ClearHeaders(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Clear();
        }

        private void SetAuthenticationHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", string.Format(@"token=""{0}""", AuthToken));
        }

        private void SetMediaTypeJsonHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypes.JsonApplication));
        }

        private StringContent GetFormattedStringContent(object resource)
        {
            //formatting to camel-casing
            var jsonObject = JsonConvert.SerializeObject(
                resource,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return new StringContent(jsonObject, Encoding.UTF8, MimeTypes.JsonApplication);

        }

        #region Resources

        private async Task<T> GetResource<T>(string path)
        {
            try
            {
                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);

                HttpResponseMessage response = await _client.GetAsync(new Uri(_baseUrl + path));

                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }

            return default(T);
        }

        private async Task<T> GetResource<T>(string path, Dictionary<string, string> urlParams)
        {
            try
            {
                var url = new Uri(_baseUrl + path);
                url = url.AddQueryParams(urlParams);

                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);

                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }

            return default(T);
        }

        private async Task<MemoryStream> GetStreamResource(string path, string mediaType = "image/*")
        {
            try
            {
                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
                var ret = new MemoryStream(); //TODO check & refactor these lines
                using (var t = await _client.GetStreamAsync(new Uri(_baseUrl + path)))
                {
                    await t.CopyToAsync(ret);
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task PutResource<T>(string path, T resource, string mediaType = "application/json")
        {
            try
            {
                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
                var content = GetFormattedStringContent(resource);

                HttpResponseMessage response = await _client.PutAsync(new Uri(_baseUrl + path), content);
                if (response.IsSuccessStatusCode)
                {
                    // server returned code 2xx, ok!
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task PutResource<T>(string path, Dictionary<string, string> urlParams, T resource)
        {
            try
            {
                var url = new Uri(_baseUrl + path);
                url = url.AddQueryParams(urlParams);

                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);
                var content = GetFormattedStringContent(resource);

                HttpResponseMessage response = await _client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    // server returned code 2xx, ok!
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<T> PutResource<T, TR>(string path, Dictionary<string, string> urlParams, TR resource)
            where T : class, new()
        {
            try
            {
                var url = new Uri(_baseUrl + path);
                url = url.AddQueryParams(urlParams);

                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);
                var content = GetFormattedStringContent(resource);

                HttpResponseMessage response = await _client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }

            return new T();
        }

        private async Task<T> PostResource<T, TR>(string path, TR resource) where T : class, new()
        {
            try
            {
                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);
                var content = GetFormattedStringContent(resource);

                HttpResponseMessage response = await _client.PostAsync(new Uri(_baseUrl + path), content);
                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }

            return new T();
        }

        private async Task PostResource<TR>(string path, Dictionary<string, string> urlParams, TR resource)
        {
            try
            {
                var url = new Uri(_baseUrl + path);
                url = url.AddQueryParams(urlParams);

                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);
                var content = GetFormattedStringContent(resource);

                HttpResponseMessage response = await _client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    //var ret = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                    //return ret;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }

            // return new T();
        }

        private async Task DeleteResource(string path)
        {
            try
            {
                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);

                HttpResponseMessage response = await _client.DeleteAsync(new Uri(_baseUrl + path));
                if (response.IsSuccessStatusCode)
                {
                    // server returned code 2xx, ok!
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task DeleteResource(string path, Dictionary<string, string> urlParams)
        {
            try
            {
                var url = new Uri(_baseUrl + path);
                url = url.AddQueryParams(urlParams);

                ClearHeaders(_client);
                SetAuthenticationHeader(_client);
                SetMediaTypeJsonHeader(_client);

                HttpResponseMessage response = await _client.DeleteAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // server returned code 2xx, ok!
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<UserInfo> GetUserInfo()
        {
            try
            {
                return await GetResource<UserInfo>("/api/v2/user");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<ConnectionList> GetUserConnections()
        {
            try
            {
                return await GetResource<ConnectionList>("/api/v2/user/connections");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateUserInfo(string firstName, string lastName)
        {
            var uu = new UserUpdate
            {
                FirstName = firstName,
                LastName = lastName
            };

            try
            {
                await PutResource("/api/v2/user", uu);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Password change

        public async Task UpdateUserPassword(string newPassword, string oldPassword)
        {
            var upu = new UserPasswordUpdate
            {
                NewPassword = newPassword,
                OldPassword = oldPassword
            };

            try
            {
                await PutResource("/api/v2/user/password", upu);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Notifications

        public async Task<NotificationSettings> GetNotificationSettings()
        {
            try
            {
                return await GetResource<NotificationSettings>("/api/v2/user/settings/notifications");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateNotificationSettings(NotificationSettings s)
        {
            try
            {
                await PutResource("/api/v2/user/settings/notifications", s);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Security

        public async Task<SecuritySettings> GetSecuritySettings()
        {
            try
            {
                return await GetResource<SecuritySettings>("/api/v2/user/settings/security");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateSecuritySettings(SecuritySettings s)
        {
            try
            {
                await PutResource("/api/v2/user/settings/security", s);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Bookmarks

        public async Task<List<Bookmark>> GetBookmarks()
        {
            try
            {
                var rv = await GetResource<BookmarkList>("/api/v2/user/bookmarks");
                return null != rv ? rv.Bookmarks : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task PutBookmarks(List<Bookmark> bookmarks)
        {
            var rv = new BookmarkList { Bookmarks = bookmarks };

            try
            {
                await PutResource("/api/v2/user/bookmarks", rv);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteBookmark(Bookmark bookmark)
        {
            List<Bookmark> bmarks = await GetBookmarks();
            bmarks.Remove(bookmark);
            await PutBookmarks(bmarks);
        }

        public async Task AddBookmark(string mountId, string path, string name)
        {
            var b = new Bookmark
            {
                MountId = mountId,
                Path = path,
                Name = name
            };
            List<Bookmark> bmarks = await GetBookmarks();
            if (!bmarks.Contains(b))
            {
                bmarks.Add(b);
                await PutBookmarks(bmarks);
            }
        }

        #endregion

        #region Profile

        public async Task<MemoryStream> GetProfilePicture(string userId)
        {
            try
            {
                return await GetStreamResource("/content/api/v2/users/" + userId + "/profile-picture");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Groups

        public async Task<Group> NewGroup(string name)
        {
            var groupName = new NameRequest { Name = name };

            try
            {
                return await PostResource<Group, NameRequest>("/api/v2/groups", groupName);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<Group>> GetGroups()
        {
            try
            {
                var rv = await GetResource<GroupList>("/api/v2/groups");
                return null != rv ? rv.Groups : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Group> GetGroup(string id)
        {
            try
            {
                return await GetResource<Group>("/api/v2/groups/" + id);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateGroup(string id, string name)
        {
            var r = new NameRequest { Name = name };
            try
            {
                await PutResource("/api/v2/groups/" + id, r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Users

        public async Task<User> AddUserToGroup(string groupId, string email, Permissions permissions)
        {
            var r = new AddUserRequest
            {
                Email = email,
                Permissions = permissions
            };

            try
            {
                return await PostResource<User, AddUserRequest>("/api/v2/groups/" + groupId + "/users", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateGroupUser(string groupId, string userId, Permissions permissions)
        {
            var r = new UpdatePermissionsRequest { Permissions = permissions };

            try
            {
                await PutResource("/api/v2/groups/" + groupId + "/users/" + userId, r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteGroupUser(string groupId, string userId)
        {
            try
            {
                await DeleteResource("/api/v2/groups/" + groupId + "/users/" + userId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteGroup(string groupId)
        {
            try
            {
                await DeleteResource("/api/v2/groups/" + groupId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Devices

        public async Task<List<Device>> GetDevices()
        {
            try
            {
                var rv = await GetResource<DeviceList>("/api/v2/devices");
                return null != rv ? rv.Devices : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Device> CreateDevice(string name, string providerName)
        {
            var r = new DeviceCreateRequest
            {
                Name = name,
                ProviderName = providerName
            };

            try
            {
                return await PostResource<Device, DeviceCreateRequest>("/api/v2/devices", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Device> GetDevice(string deviceId)
        {
            try
            {
                return await GetResource<Device>("/api/v2/devices/" + deviceId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateDevice(string deviceId, string newName)
        {
            var r = new NameRequest { Name = newName };
            try
            {
                await PutResource("/api/v2/devices/" + deviceId, r); //.put(r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteDevice(string deviceId)
        {
            try
            {
                await DeleteResource("/api/v2/devices/" + deviceId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Mounts Folders Files

        public async Task<List<Mount>> GetMounts()
        {
            try
            {
                var rv = await GetResource<MountList>("/api/v2/mounts");
                return null != rv ? rv.Mounts : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Mount> GetMount(string id)
        {
            try
            {
                return await GetResource<Mount>("/api/v2/mounts/" + id);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Mount> CreateMount(string parentId, string path, string name)
        {
            var r = new CreateMountRequest
            {
                Name = name,
                Path = path
            };

            try
            {
                return await PostResource<Mount, CreateMountRequest>("/api/v2/mounts/" + parentId + "/submounts", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateMount(string id, string newName)
        {
            var r = new NameRequest { Name = newName };

            try
            {
                await PutResource("/api/v2/mounts/" + id, r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<User> AddUserToMount(string mountId, string email, Permissions perm)
        {
            var r = new AddUserRequest
            {
                Email = email,
                Permissions = perm
            };
            try
            {
                return await PostResource<User, AddUserRequest>("/api/v2/mounts/" + mountId + "/users", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateMountUser(string mountId, string userId, Permissions perm)
        {
            var r = new UpdatePermissionsRequest { Permissions = perm };

            try
            {
                await PutResource("/api/v2/mounts/" + mountId + "/users/" + userId, r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteMountUser(string mountId, string userId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/users/" + userId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task UpdateMountGroup(string mountId, string groupId, Permissions perm)
        {
            var r = new UpdatePermissionsRequest { Permissions = perm };
            try
            {
                await PutResource("/api/v2/mounts/" + mountId + "/groups/" + groupId, r); //.put(r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteMountGroup(string mountId, string groupId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/groups/" + groupId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteMount(string mountId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task CreateFolder(string mountId, string path, string folderName)
        {
            var r = new NameRequest { Name = folderName };
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                await PostResource("/api/v2/mounts/" + mountId + "/files/folder", q, r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<string> GetUploadUrl(string mountId, string path)
        {
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                var fl = await GetResource<FileLink>("/api/v2/mounts/" + mountId + "/files/upload", q);
                return null != fl ? fl.Link : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<string> GetDownloadUrl(string mountId, string path)
        {
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                var fl = await GetResource<FileLink>("/api/v2/mounts/" + mountId + "/files/download", q);
                return fl != null ? fl.Link : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<File>> ListFiles(string mountId, string path)
        {
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                var fl = await GetResource<FileList>("/api/v2/mounts/" + mountId + "/files/list", q);
                return null != fl ? fl.Files : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<PathInfo> GetPathInfo(string mountId, string path)
        {
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                return await GetResource<PathInfo>("/api/v2/mounts/" + mountId + "/bundle", q);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<File> GetFileInfo(string mountId, string path)
        {
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                return await GetResource<File>("/api/v2/mounts/" + mountId + "/files/info", q);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task RenamePath(string mountId, string path, string newName)
        {
            var r = new NameRequest { Name = newName };

            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                await PutResource("/api/v2/mounts/" + mountId + "/files/rename", q, r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task RemovePath(string mountId, string path)
        {
            try
            {
                var q = new Dictionary<string, string> { { "path", path } };
                await DeleteResource("/api/v2/mounts/" + mountId + "/files/remove", q);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<string> CopyPath(string srcMountId, string srcPath, string dstMountId, string dstPath)
        {
            try
            {
                var r = new DestinationRequest { ToMountId = dstMountId, ToPath = dstPath };
                var rv =
                    await
                        PutResource<NameRequest, DestinationRequest>("/api/v2/mounts/" + srcMountId + "/files/copy",
                            new Dictionary<string, string> { { "path", srcPath } }, r);
                return null != rv ? rv.Name : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<string> MovePath(string srcMountId, string srcPath, string dstMountId, string dstPath)
        {

            try
            {
                var r = new DestinationRequest { ToMountId = dstMountId, ToPath = dstPath };
                var rv = await PutResource<NameRequest, DestinationRequest>("/api/v2/mounts/" + srcMountId + "/files/move",
                    new Dictionary<string, string> { { "path", srcPath } }, r);
                return null != rv ? rv.Name : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Comments

        public async Task<Comment> PostComment(string mountId, string content)
        {
            var r = new PostCommentRequest { Content = content };
            try
            {
                return await PostResource<Comment, PostCommentRequest>("/api/v2/mounts/" + mountId + "/comments", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<Comment>> GetComments(string mountId)
        {
            return await GetComments(mountId, -1, -1);
        }

        public async Task<List<Comment>> GetComments(string mountId, int from, int limit)
        {
            try
            {
                if (from >= 0 && limit > 0)
                {
                    var queryString = new Dictionary<string, string>
                    {
                        {"from", from.ToString(CultureInfo.InvariantCulture)},
                        {"limit", limit.ToString(CultureInfo.InvariantCulture)}
                    };

                    var ret = await GetResource<CommentList>("/api/v2/mounts/" + mountId + "/comments", queryString);
                    return ret.Comments;
                }
                else
                {
                    var ret = await GetResource<CommentList>("/api/v2/mounts/" + mountId + "/comments");
                    return ret.Comments;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Comment> GetComment(string mountId, string commentId)
        {
            try
            {
                return await GetResource<Comment>("/api/v2/mounts/" + mountId + "/comments/" + commentId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteComment(string mountId, string commentId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/comments/" + commentId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Links

        public async Task<Link> CreateReceiver(string mountId, string path)
        {
            var r = new PathRequest { Path = path };

            try
            {
                return await PostResource<Link, PathRequest>("/api/v2/mounts/" + mountId + "/receivers", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<Link>> GetReceivers(string mountId)
        {
            try
            {
                var ll = await GetResource<LinkList>("/api/v2/mounts/" + mountId);
                return ll != null ? ll.Links : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Link> GetReceiver(string mountId, string linkId)
        {
            try
            {
                return await GetResource<Link>("/api/v2/mounts/" + mountId + "/receivers/" + linkId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Link> GetReceiverForPath(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            try
            {
                return await GetResource<Link>("/api/v2/mounts/" + mountId + "/receivers", q);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<Link>> GetReceiversInParent(string mountId, string parent)
        {
            var q = new Dictionary<string, string> { { "parent", parent } };
            try
            {
                var ll = await GetResource<LinkList>("/api/v2/mounts/" + mountId + "/receivers", q);
                return null != ll ? ll.Links : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region N

        public async Task SetReceiverUrlPath(string mountId, string linkId, string path)
        {
            var r = new HashRequest { Hash = path };

            try
            {
                await PutResource("/api/v2/mounts/" + mountId + "/receivers/" + linkId + "/urlHash", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        //public async Task<Link> ResetReceiverPassword(string mountId, string linkId)
        //{
        //    try
        //    {
        //        return await PutResource<Link, Link>("/api/v2/mounts/" + mountId + "/receivers/" + linkId + "/password/reset",  null as Link); //.put(null, typeof (Link));
        //    }
        //    catch ( /*ResourceException*/ Exception ex)
        //    {
        //        throw new KoofrException(ex);
        //    }
        //}

        public async Task DeleteReceiverPassword(string mountId, string linkId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/receivers/" + linkId + "/password");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteReceiver(string mountId, string linkId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/receivers/" + linkId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Link> CreateLink(string mountId, string path)
        {
            var r = new PathRequest { Path = path };

            try
            {
                return await PostResource<Link, PathRequest>("/api/v2/mounts/" + mountId + "/links", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<Link>> GetLinks(string mountId)
        {
            try
            {
                var ll = await GetResource<LinkList>("/api/v2/mounts/" + mountId);
                return null != ll ? ll.Links : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Link> GetLink(string mountId, string linkId)
        {
            try
            {
                return await GetResource<Link>("/api/v2/mounts/" + mountId + "/links/" + linkId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Link> GetLinkForPath(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            try
            {
                return await GetResource<Link>("/api/v2/mounts/" + mountId + "/links", q);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<List<Link>> GetLinksInParent(string mountId, string parent)
        {
            var q = new Dictionary<string, string> { { "parent", parent } };
            try
            {
                LinkList ll = await GetResource<LinkList>("/api/v2/mounts/" + mountId + "/links", q);
                return null != ll ? ll.Links : null;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task SetLinkUrlPath(string mountId, string linkId, string path)
        {
            var r = new HashRequest { Hash = path };
            try
            {
                await PutResource("/api/v2/mounts/" + mountId + "/links/" + linkId + "/urlHash", r);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

       //public async Task<Link> ResetLinkPassword(string mountId, string linkId)
       // {
       //     try
       //     {
       //         await PutResource<Link>("/api/v2/mounts/" + mountId + "/links/" + linkId + "/password/reset", null);
       //     }
       //     catch (/*ResourceException*/ Exception ex)
       //     {
       //         throw new KoofrException(ex);
       //     }
       // }

        public async Task DeleteLinkPassword(string mountId, string linkId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/links/" + linkId + "/password");
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task DeleteLink(string mountId, string linkId)
        {
            try
            {
                await DeleteResource("/api/v2/mounts/" + mountId + "/links/" + linkId);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion

        #region Download/upload

        public bool CheckAuthentication()
        {
            try
            {
                return GetUserInfo().Result != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> FilesUpload(string mountId, string path, String name, byte[] data)
        {
            var stream = new MemoryStream(data);

            return await FilesUpload(mountId, path, name, stream);
        }

        public async Task<bool> FilesUpload(string mountId, string path, String name, Stream data)
        {
            string uploadUrl = _baseUrl + "/content/api/v2/mounts/" + mountId + "/files/put?path=" + WebUtility.UrlEncode(path);

            try
            {
                using (var httpClient = new HttpClient(new HttpClientHandler()))
                {
                    if (AuthToken != null)
                    {
                        SetAuthenticationHeader(httpClient);
                    }
                    else
                    {
                        throw new KoofrException("No authorization token.");
                    }

                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StreamContent(data), "file", name);
                        HttpResponseMessage response = await httpClient.PostAsync(uploadUrl, content);
                        HttpResponseMessage message = response.EnsureSuccessStatusCode();
                        return message.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<Stream> FilesDownload(string @base, string path)
        {
            return await FilesDownload(@base, path, null);
        }

        public async Task<Stream> FilesDownload(string mountId, string path, string thumbSize)
        {
            try
            {
                string downloadUrl = _baseUrl + "/content/api/v2/mounts/" + mountId + "/files/get?path=" + WebUtility.UrlEncode(path);

                if (thumbSize != null)
                {
                    downloadUrl += "&thumb=" + thumbSize;
                }

                using (var httpClient = new HttpClient(new HttpClientHandler()))
                {
                    if (AuthToken != null)
                    {
                        SetAuthenticationHeader(httpClient);
                    }
                    else
                    {
                        throw new KoofrException("No authorization token.");
                    }

                    HttpResponseMessage response = await httpClient.GetAsync(downloadUrl);
                    HttpResponseMessage message = response.EnsureSuccessStatusCode();
                    if (message.IsSuccessStatusCode)
                    {
                        return await message.Content.ReadAsStreamAsync();
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<byte[]> FilesDownloadBytes(string mountId, string path, string thumbSize)
        {
            try
            {
                var stream = await FilesDownload(mountId, path, thumbSize);

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        #endregion
    }
}