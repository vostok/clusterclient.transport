using System;
using System.Net.Http;

namespace Vostok.Clusterclient.Transport.Native
{
    internal static class CurlExceptionHelper
    {
        private static readonly Type CurlExceptionType = typeof(HttpClient).Assembly.GetType("System.Net.Http.CurlException");

        public static bool IsCurlException(Exception error, out CurlCode code)
        {
            code = (CurlCode)(error?.HResult ?? 0);

            return error != null && error.GetType() == CurlExceptionType;
        }

        public static bool IsConnectionFailure(CurlCode code)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (code)
            {
                case CurlCode.CURLE_COULDNT_RESOLVE_PROXY:
                case CurlCode.CURLE_COULDNT_RESOLVE_HOST:
                case CurlCode.CURLE_COULDNT_CONNECT:

                case CurlCode.CURLE_SSL_CIPHER:
                case CurlCode.CURLE_SSL_CERTPROBLEM:
                case CurlCode.CURLE_SSL_CONNECT_ERROR:
                case CurlCode.CURLE_SSL_ENGINE_NOTFOUND:
                    return true;
            }

            return false;
        }
    }
}
