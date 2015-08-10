namespace Koofr.Sdk.Api.V2.Resources
{
    public class Bookmark
    {
        public string Name { get; set; }
        public string MountId { get; set; }
        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Bookmark)obj);
        }

        protected bool Equals(Bookmark other)
        {
            return
                string.Equals(Name, other.Name) &&
                string.Equals(MountId, other.MountId) &&
                string.Equals(Path, other.Path);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MountId != null ? MountId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}