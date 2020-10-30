using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Contents
{
    internal class ContentHeadersHelper
    {
        private static volatile Action<HttpContent, HttpContent> copier;

        static ContentHeadersHelper()
        {
            try
            {
                var headersField = typeof(HttpContent).GetField("_headers", BindingFlags.Instance | BindingFlags.NonPublic);
                if (headersField == null)
                    return;

                var fromParameter = Expression.Parameter(typeof(HttpContent));
                var fromField = Expression.Field(fromParameter, headersField);

                var toParameter = Expression.Parameter(typeof(HttpContent));
                var toField = Expression.Field(toParameter, headersField);

                var assignment = Expression.Assign(toField, fromField);

                copier = Expression.Lambda<Action<HttpContent, HttpContent>>(assignment, fromParameter, toParameter).Compile();
            }
            catch (Exception error)
            {
                LogProvider.Get().ForContext<ContentHeadersHelper>().Warn(error);
            }
        }

        public static bool TryCopyByReference(HttpContent from, HttpContent to)
        {
            if (copier == null)
                return false;

            try
            {
                copier(from, to);
                return true;
            }
            catch (Exception error)
            {
                LogProvider.Get().ForContext<ContentHeadersHelper>().Warn(error);
                copier = null;
                return false;
            }
        }
    }
}
