﻿using System.IO;
using System.Threading.Tasks;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Contents
{
    internal class EmptyContent : GenericContent
    {
        public EmptyContent()
        {
            Headers.ContentLength = 0;
        }

        public override long? Length => 0;

        public override Stream AsStream => Stream.Null;

        public override Task Copy(Stream target) => Task.CompletedTask;
    }
}
