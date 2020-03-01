using System.Text;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    // (iloktionov): A dirty hack that exploits header serialization implementation inside HttpWebRequest
    // (iloktionov): (it just directly casts every character to byte, so we can cook a longer
    // (iloktionov): ASCII string from initial UTF-8 string to obtain correct byte representation)
    internal static class NonAsciiHeadersFixer
    {
        private const char MinASCII = '\u0020';
        private const char MaxASCII = '\u007E';

        public static Request Fix(Request request)
        {
            var headers = request.Headers;
            if (headers == null)
                return request;

            foreach (var header in headers)
            {
                if (IsAscii(header.Value))
                    continue;

                headers = headers.Set(header.Name, FixRequestHeaderValue(header.Value));
            }

            if (!ReferenceEquals(headers, request.Headers))
                request = request.WithHeaders(headers);

            return request;
        }

        public static string FixResponseHeaderValue(string value)
        {
            if (IsAscii(value))
                return value;

            var intermediateBuffer = new byte[value.Length];

            for (var i = 0; i < value.Length; i++)
            {
                intermediateBuffer[i] = (byte)value[i];
            }

            return Encoding.UTF8.GetString(intermediateBuffer);
        }

        private static string FixRequestHeaderValue(string value)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(value);

            var fixedStringBuilder = new StringBuilder(utf8Bytes.Length);

            for (var i = 0; i < utf8Bytes.Length; i++)
            {
                fixedStringBuilder.Append((char)utf8Bytes[i]);
            }

            return fixedStringBuilder.ToString();
        }

        private static bool IsAscii(string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var currentCharacter = value[i];
                if (currentCharacter < MinASCII || currentCharacter > MaxASCII)
                    return false;
            }

            return true;
        }
    }
}
