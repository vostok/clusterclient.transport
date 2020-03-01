using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.BodyReading
{
    internal interface IBodyReader
    {
        Task<BodyReadResult> ReadAsync(HttpResponseMessage message, CancellationToken cancellationToken);
    }
}
