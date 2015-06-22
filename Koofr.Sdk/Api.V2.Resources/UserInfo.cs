using System.Text;

namespace Koofr.Sdk.Api.V2.Resources
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string FormattedName
        {
            get
            {
                var sb = new StringBuilder();
                if (null != FirstName)
                {
                    sb.Append(FirstName).Append(" ");
                }
                if (null != LastName)
                {
                    sb.Append(LastName);
                }
                if (sb.Length == 0)
                {
                    sb.Append(Email);
                }
                return sb.ToString();
            }
        }
    }
}