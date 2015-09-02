using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Koofr.Sdk.Api.V2.Resources;
using Koofr.Sdk.Api.V2.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using File = Koofr.Sdk.Api.V2.Resources.File;

namespace Koofr.Sdk.Api.V2
{
    public class OAuthSettings
    {
        public string Scope { get; set; }
        public string RedirectUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class OAuthToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }

        public Boolean IsExpired()
        {
            return DateTime.Now > ExpiresAt;
        }
    }

    public class OAuthResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }

    public partial class KoofrClient
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client;
        private readonly OAuthSettings _oauthSettings;

        public OAuthToken OAuthToken { get; set; }

        public Func<Task> OnUnauthorized { get; set; }

        public KoofrClient(String baseUrl, HttpClient client, OAuthSettings oauthSettings)
        {
            _baseUrl = baseUrl;
            _client = client;
            _oauthSettings = oauthSettings;
        }

        public void SetOAuthToken(OAuthResponse resp)
        {
            var expiresAt = DateTime.Now.AddSeconds(resp.ExpiresIn);

            OAuthToken = new OAuthToken
            {
                AccessToken = resp.AccessToken,
                RefreshToken = resp.RefreshToken,
                ExpiresAt = expiresAt
            };
        }

        protected async Task FetchToken(Dictionary<string, string> urlParams)
        {
            var path = "/oauth2/token";

            var uri = new Uri(_baseUrl + path);
            uri = uri.AddQueryParams(urlParams);

            var content = new ByteArrayContent(new byte[] { });

            var resp = await _client.PostAsync(uri, content);

            var respStr = resp.Content.ReadAsStringAsync().Result;

            var ret = JsonConvert.DeserializeObject<OAuthResponse>(respStr);

            if (ret.Error != null)
            {
                throw new KoofrException("Authentication error: " + ret.Error + " (" + ret.ErrorDescription + ")");
            }

            SetOAuthToken(ret);
        }

        public async Task Authenticate(string code)
        {
            var urlParams = new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", _oauthSettings.RedirectUri},
                {"client_id", _oauthSettings.ClientId},
                {"client_secret", _oauthSettings.ClientSecret}
            };

            await FetchToken(urlParams);
        }

        public async Task AuthenticateRefreshToken()
        {
            var urlParams = new Dictionary<string, string>
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", OAuthToken.RefreshToken},
                {"redirect_uri", _oauthSettings.RedirectUri},
                {"client_id", _oauthSettings.ClientId},
                {"client_secret", _oauthSettings.ClientSecret}
            };

            await FetchToken(urlParams);
        }

        private void SetMediaTypeJsonHeader(HttpRequestMessage req)
        {
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypes.JsonApplication));
        }

        private async Task PrepareRequest(HttpRequestMessage req)
        {
            if (OAuthToken.IsExpired())
            {
                try
                {
                    await AuthenticateRefreshToken();
                }
                catch (Exception e)
                {
                    if (OnUnauthorized != null)
                    {
                        await OnUnauthorized();
                    }
                }
            }

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", OAuthToken.AccessToken);
        }

        private async Task HandleResponse(HttpResponseMessage res)
        {
            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (OnUnauthorized != null)
                {
                    await OnUnauthorized();
                }
            }
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
                var uri = new Uri(_baseUrl + path);

                var req = new HttpRequestMessage(HttpMethod.Get, uri);

                SetMediaTypeJsonHeader(req);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<T> GetResource<T>(string path, Dictionary<string, string> urlParams)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Get, uri);

                SetMediaTypeJsonHeader(req);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<Stream> GetStreamResource(string path, Dictionary<string, string> urlParams)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Get, uri);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                return stream;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<MemoryStream> GetMemoryStreamResource(string path, Dictionary<string, string> urlParams)
        {
            try
            {
                var ret = new MemoryStream();

                using (var t = await GetStreamResource(path, urlParams))
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

        private async Task PutResource<T>(string path, T resource)
        {
            await PutResource(path, new Dictionary<string, string> { }, resource);
        }

        private async Task PutResource<T>(string path, Dictionary<string, string> urlParams, T resource)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Put, uri);

                SetMediaTypeJsonHeader(req);

                req.Content = GetFormattedStringContent(resource);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<T> PutResource<T, TR>(string path, Dictionary<string, string> urlParams, TR resource)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Put, uri);

                SetMediaTypeJsonHeader(req);

                req.Content = GetFormattedStringContent(resource);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<T> PostResource<T, TR>(string path, TR resource)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);

                var req = new HttpRequestMessage(HttpMethod.Post, uri);

                SetMediaTypeJsonHeader(req);

                req.Content = GetFormattedStringContent(resource);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task PostResource<TR>(string path, Dictionary<string, string> urlParams, TR resource)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Post, uri);

                SetMediaTypeJsonHeader(req);

                req.Content = GetFormattedStringContent(resource);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task<HttpResponseMessage> PostContent(string path, Dictionary<string, string> urlParams, HttpContent content)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Post, uri);

                req.Content = content;

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        private async Task DeleteResource(string path)
        {
            await DeleteResource(path, new Dictionary<string, string> { });
        }

        private async Task DeleteResource(string path, Dictionary<string, string> urlParams)
        {
            try
            {
                var uri = new Uri(_baseUrl + path);
                uri = uri.AddQueryParams(urlParams);

                var req = new HttpRequestMessage(HttpMethod.Delete, uri);

                SetMediaTypeJsonHeader(req);

                await PrepareRequest(req);

                HttpResponseMessage response = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);

                await HandleResponse(response);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new KoofrException(ex);
            }
        }

        public async Task<UserInfo> GetUserInfo()
        {
            return await GetResource<UserInfo>("/api/v2/user");
        }

        public async Task<ConnectionList> GetUserConnections()
        {
            return await GetResource<ConnectionList>("/api/v2/user/connections");
        }

        public async Task UpdateUserInfo(string firstName, string lastName)
        {
            var uu = new UserUpdate
            {
                FirstName = firstName,
                LastName = lastName
            };

            await PutResource("/api/v2/user", uu);
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

            await PutResource("/api/v2/user/password", upu);
        }

        #endregion

        #region Notifications

        public async Task<NotificationSettings> GetNotificationSettings()
        {
            return await GetResource<NotificationSettings>("/api/v2/user/settings/notifications");
        }

        public async Task UpdateNotificationSettings(NotificationSettings s)
        {
            await PutResource("/api/v2/user/settings/notifications", s);
        }

        #endregion

        #region Security

        public async Task<SecuritySettings> GetSecuritySettings()
        {
            return await GetResource<SecuritySettings>("/api/v2/user/settings/security");
        }

        public async Task UpdateSecuritySettings(SecuritySettings s)
        {
            await PutResource("/api/v2/user/settings/security", s);
        }

        #endregion

        #region Bookmarks

        public async Task<List<Bookmark>> GetBookmarks()
        {
            return (await GetResource<BookmarkList>("/api/v2/user/bookmarks")).Bookmarks;
        }

        public async Task PutBookmarks(List<Bookmark> bookmarks)
        {
            var rv = new BookmarkList { Bookmarks = bookmarks };

            await PutResource("/api/v2/user/bookmarks", rv);
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
            return await GetMemoryStreamResource("/content/api/v2/users/" + userId + "/profile-picture", new Dictionary<string, string> { });
        }

        #endregion

        #region Groups

        public async Task<Group> NewGroup(string name)
        {
            var groupName = new NameRequest { Name = name };

            return await PostResource<Group, NameRequest>("/api/v2/groups", groupName);
        }

        public async Task<List<Group>> GetGroups()
        {
            return (await GetResource<GroupList>("/api/v2/groups")).Groups;
        }

        public async Task<Group> GetGroup(string id)
        {
            return await GetResource<Group>("/api/v2/groups/" + id);
        }

        public async Task UpdateGroup(string id, string name)
        {
            var r = new NameRequest { Name = name };

            await PutResource("/api/v2/groups/" + id, r);
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

            return await PostResource<User, AddUserRequest>("/api/v2/groups/" + groupId + "/users", r);
        }

        public async Task UpdateGroupUser(string groupId, string userId, Permissions permissions)
        {
            var r = new UpdatePermissionsRequest { Permissions = permissions };

            await PutResource("/api/v2/groups/" + groupId + "/users/" + userId, r);
        }

        public async Task DeleteGroupUser(string groupId, string userId)
        {
            await DeleteResource("/api/v2/groups/" + groupId + "/users/" + userId);
        }

        public async Task DeleteGroup(string groupId)
        {
            await DeleteResource("/api/v2/groups/" + groupId);
        }

        #endregion

        #region Devices

        public async Task<List<Device>> GetDevices()
        {
            return (await GetResource<DeviceList>("/api/v2/devices")).Devices;
        }

        public async Task<Device> CreateDevice(string name, string providerName)
        {
            var r = new DeviceCreateRequest
            {
                Name = name,
                ProviderName = providerName
            };

            return await PostResource<Device, DeviceCreateRequest>("/api/v2/devices", r);
        }

        public async Task<Device> GetDevice(string deviceId)
        {
            return await GetResource<Device>("/api/v2/devices/" + deviceId);
        }

        public async Task UpdateDevice(string deviceId, string newName)
        {
            var r = new NameRequest { Name = newName };

            await PutResource("/api/v2/devices/" + deviceId, r);
        }

        public async Task DeleteDevice(string deviceId)
        {
            await DeleteResource("/api/v2/devices/" + deviceId);
        }

        #endregion

        #region Mounts Folders Files

        public async Task<List<Mount>> GetMounts()
        {
            return (await GetResource<MountList>("/api/v2/mounts")).Mounts;
        }

        public async Task<Mount> GetMount(string id)
        {
            return await GetResource<Mount>("/api/v2/mounts/" + id);
        }

        public async Task<Mount> CreateMount(string parentId, string path, string name)
        {
            var r = new CreateMountRequest
            {
                Name = name,
                Path = path
            };

            return await PostResource<Mount, CreateMountRequest>("/api/v2/mounts/" + parentId + "/submounts", r);
        }

        public async Task UpdateMount(string id, string newName)
        {
            var r = new NameRequest { Name = newName };

            await PutResource("/api/v2/mounts/" + id, r);
        }

        public async Task<User> AddUserToMount(string mountId, string email, Permissions perm)
        {
            var r = new AddUserRequest
            {
                Email = email,
                Permissions = perm
            };
            return await PostResource<User, AddUserRequest>("/api/v2/mounts/" + mountId + "/users", r);
        }

        public async Task UpdateMountUser(string mountId, string userId, Permissions perm)
        {
            var r = new UpdatePermissionsRequest { Permissions = perm };

            await PutResource("/api/v2/mounts/" + mountId + "/users/" + userId, r);
        }

        public async Task DeleteMountUser(string mountId, string userId)
        {
            await DeleteResource("/api/v2/mounts/" + mountId + "/users/" + userId);
        }

        public async Task UpdateMountGroup(string mountId, string groupId, Permissions perm)
        {
            var r = new UpdatePermissionsRequest { Permissions = perm };

            await PutResource("/api/v2/mounts/" + mountId + "/groups/" + groupId, r); //.put(r);
        }

        public async Task DeleteMountGroup(string mountId, string groupId)
        {
            await DeleteResource("/api/v2/mounts/" + mountId + "/groups/" + groupId);
        }

        public async Task DeleteMount(string mountId)
        {
            await DeleteResource("/api/v2/mounts/" + mountId);
        }

        public async Task CreateFolder(string mountId, string path, string folderName)
        {
            var r = new NameRequest { Name = folderName };

            var q = new Dictionary<string, string> { { "path", path } };

            await PostResource("/api/v2/mounts/" + mountId + "/files/folder", q, r);
        }

        public async Task<string> GetUploadUrl(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            return (await GetResource<FileLink>("/api/v2/mounts/" + mountId + "/files/upload", q)).Link;
        }

        public async Task<string> GetDownloadUrl(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            return (await GetResource<FileLink>("/api/v2/mounts/" + mountId + "/files/download", q)).Link;
        }

        public async Task<List<File>> ListFiles(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            return (await GetResource<FileList>("/api/v2/mounts/" + mountId + "/files/list", q)).Files;
        }

        public async Task<PathInfo> GetPathInfo(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            return await GetResource<PathInfo>("/api/v2/mounts/" + mountId + "/bundle", q);
        }

        public async Task<File> GetFileInfo(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            return await GetResource<File>("/api/v2/mounts/" + mountId + "/files/info", q);
        }

        public async Task RenamePath(string mountId, string path, string newName)
        {
            var r = new NameRequest { Name = newName };

            var q = new Dictionary<string, string> { { "path", path } };
            await PutResource("/api/v2/mounts/" + mountId + "/files/rename", q, r);
        }

        public async Task RemovePath(string mountId, string path)
        {
            var q = new Dictionary<string, string> { { "path", path } };
            await DeleteResource("/api/v2/mounts/" + mountId + "/files/remove", q);
        }

        public async Task<string> CopyPath(string srcMountId, string srcPath, string dstMountId, string dstPath)
        {
            var r = new DestinationRequest { ToMountId = dstMountId, ToPath = dstPath };
            var q = new Dictionary<string, string> { { "path", srcPath } };
            return (await PutResource<NameRequest, DestinationRequest>("/api/v2/mounts/" + srcMountId + "/files/copy", q, r)).Name;
        }

        public async Task<string> MovePath(string srcMountId, string srcPath, string dstMountId, string dstPath)
        {
            var r = new DestinationRequest { ToMountId = dstMountId, ToPath = dstPath };
            var q = new Dictionary<string, string> { { "path", srcPath } };
            return (await PutResource<NameRequest, DestinationRequest>("/api/v2/mounts/" + srcMountId + "/files/move", q, r)).Name;
        }

        #endregion

        #region Comments

        public async Task<Comment> PostComment(string mountId, string content)
        {
            var r = new PostCommentRequest { Content = content };

            return await PostResource<Comment, PostCommentRequest>("/api/v2/mounts/" + mountId + "/comments", r);
        }

        public async Task<List<Comment>> GetComments(string mountId)
        {
            return await GetComments(mountId, -1, -1);
        }

        public async Task<List<Comment>> GetComments(string mountId, int from, int limit)
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

        public async Task<Comment> GetComment(string mountId, string commentId)
        {
            return await GetResource<Comment>("/api/v2/mounts/" + mountId + "/comments/" + commentId);
        }

        public async Task DeleteComment(string mountId, string commentId)
        {
            await DeleteResource("/api/v2/mounts/" + mountId + "/comments/" + commentId);
        }

        #endregion

        #region Links

        public async Task<Link> CreateReceiver(string mountId, string path)
        {
            var r = new PathRequest { Path = path };

            return await PostResource<Link, PathRequest>("/api/v2/mounts/" + mountId + "/receivers", r);
        }

        #endregion

        #region Receivers

        public async Task<Link> CreateLink(string mountId, string path)
        {
            var r = new PathRequest { Path = path };

            return await PostResource<Link, PathRequest>("/api/v2/mounts/" + mountId + "/links", r);
        }

        #endregion

        #region Download/upload

        public async Task<bool> FilesUpload(string mountId, string path, String name, byte[] data)
        {
            var stream = new MemoryStream(data);

            return await FilesUpload(mountId, path, name, stream);
        }

        public async Task<bool> FilesUpload(string mountId, string path, String name, Stream data)
        {
            string reqPath = "/content/api/v2/mounts/" + mountId + "/files/put";
            var urlParams = new Dictionary<string, string> { { "path", path } };

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StreamContent(data), "file", name);

                HttpResponseMessage response = await PostContent(reqPath, urlParams, content);

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<Stream> FilesDownload(string @base, string path)
        {
            return await FilesDownload(@base, path, null);
        }

        public async Task<Stream> FilesDownload(string mountId, string path, string thumbSize)
        {
            var urlParams = new Dictionary<string, string> { { "path", path } };

            if (thumbSize != null)
            {
                urlParams["thumb"] = thumbSize;
            }

            Stream stream = await GetStreamResource("/content/api/v2/mounts/" + mountId + "/files/get", urlParams);

            return stream;
        }

        public async Task<byte[]> FilesDownloadBytes(string mountId, string path, string thumbSize)
        {
            var stream = await FilesDownload(mountId, path, thumbSize);

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        #endregion
    }
}