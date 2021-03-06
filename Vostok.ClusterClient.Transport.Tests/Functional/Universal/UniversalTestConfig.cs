﻿using System;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Universal
{
    internal class UniversalTestConfig : ITransportTestConfig
    {
        public ITransport CreateTransport(UniversalTransportSettings settings, ILog log) 
            => new UniversalTransport(settings, log);

        public Runtime Runtimes
        {
            get
            {
                var runtimes = Runtime.Framework | Runtime.Core21 | Runtime.Core31 | Runtime.Core50;

                if (Environment.GetEnvironmentVariable("APPVEYOR") == null)
                    runtimes |= Runtime.Core20;

                return runtimes;
            }
        }
    }
}
