using JetBrains.Annotations;

namespace System.Net.Http
{
    [CanBeNull] internal delegate System.Text.Encoding HeaderEncodingSelector<TContext>(string headerName, TContext context);
}