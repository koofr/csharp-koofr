using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Koofr.Sdk.Api.V2.Util
{
    public static class HttpExtensions
    {
        public static Uri AddQueryParams(this Uri uri, Dictionary<string, string> queryParams)
        {
            if (queryParams.Count < 1) return null;

            var ub = new UriBuilder(uri);

            var queryParamArraay = queryParams.Select(
                queryParam => string.Format("{0}={1}", WebUtility.UrlEncode(queryParam.Key), WebUtility.UrlEncode(queryParam.Value))).ToList();

            ub.Query = string.Join("&", queryParamArraay);
            return ub.Uri;
        }
    }
}