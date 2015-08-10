using System;

namespace Koofr.Sdk.Api.V2
{
    public class KoofrException : Exception
    {
        public KoofrException()
            : this(null, null)
        {
        }

        public KoofrException(string detailMessage)
            : this(detailMessage, null)
        {
        }

        public KoofrException(Exception exception)
            : base(null, exception)
        {
        }

        public KoofrException(string detailMessage, Exception exception)
            : base(detailMessage, exception)
        {
        }
    }
}
