using System;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class SecuritySettings
    {
        public bool DownloadLinkAutoPassword;
        public bool DownloadLinkRequirePassword;
        public bool UploadLinkAutoPassword;
        public bool UploadLinkRequirePassword;

        public SecuritySettings()
        {
        }

        public SecuritySettings(SecuritySettings src)
        {
            DownloadLinkAutoPassword = src.DownloadLinkAutoPassword;
            DownloadLinkRequirePassword = src.DownloadLinkRequirePassword;
            UploadLinkAutoPassword = src.UploadLinkAutoPassword;
            UploadLinkRequirePassword = src.UploadLinkRequirePassword;
        }

        public Object Clone()
        {
            return new SecuritySettings(this);
        }
    }
}