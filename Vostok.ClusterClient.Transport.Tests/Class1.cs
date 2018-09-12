using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Transport.Tests
{
    public class Tests
    {
        [Test, Explicit]
        public async Task Get()
        {
            var response = await new UniversalTransport(new UniversalTransportSettings(), new SilentLog()).SendAsync(
                Request.Get("http://vostok.tools/"),
                TimeSpan.FromSeconds(5),
                CancellationToken.None
            );

            var content = response.Content;

            var text = Encoding.UTF8.GetString(content.Buffer, content.Offset, content.Length);
            
            Console.WriteLine(text);
        }
    }
}