﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Native
{
    internal static class WinHttpHandlerTuner
    {
        private const BindingFlags PrivateBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static volatile Func<HttpClientHandler, SafeHandle> handleExtractor;
        private static volatile bool canTune = true;

        public static void Tune(HttpClientHandler handler, TimeSpan? connectTimeout, ILog log)
        {
            if (!canTune)
                return;

            try
            {
                if (handleExtractor == null)
                    handleExtractor = BuildHandleExtractor(log);

                var handle = handleExtractor(handler);
                if (handle == null)
                    return;

                if (!WinHttpSetTimeouts(handle, 0, (int)(connectTimeout?.TotalMilliseconds ?? 0), 0, 0))
                    throw new Win32Exception();
            }
            catch (Exception error)
            {
                canTune = false;
                log.Warn(error, "Failed to tune WinHttpHandler.");
            }
        }

        private static Func<HttpClientHandler, SafeHandle> BuildHandleExtractor(ILog log)
        {
            try
            {
                var winHttpHandlerField = typeof(HttpClientHandler).GetField("_winHttpHandler", PrivateBindingFlags);
                if (winHttpHandlerField == null)
                    return _ => null;

                var ensureSessionMethod = winHttpHandlerField.FieldType.GetMethod("EnsureSessionHandleExists", PrivateBindingFlags);
                if (ensureSessionMethod == null)
                    return _ => null;

                var requestStateType = ensureSessionMethod.GetParameters().FirstOrDefault()?.ParameterType;
                if (requestStateType == null)
                    return _ => null;

                var sessionHandleField = winHttpHandlerField.FieldType.GetField("_sessionHandle", PrivateBindingFlags);
                if (sessionHandleField == null)
                    return _ => null;

                var httpClientHandler = Expression.Parameter(typeof(HttpClientHandler));
                var winHttpHandler = Expression.Field(httpClientHandler, winHttpHandlerField);
                var sessionHandle = Expression.Field(winHttpHandler, sessionHandleField);
                var requestState = Expression.New(requestStateType.GetConstructors().First());
                var ensureSessionCall = Expression.Call(winHttpHandler, ensureSessionMethod, requestState);
                var ensureSessionAndReturn = Expression.Block(ensureSessionCall, sessionHandle);

                return Expression.Lambda<Func<HttpClientHandler, SafeHandle>>(ensureSessionAndReturn, httpClientHandler).Compile();
            }
            catch (Exception error)
            {
                log.Error(error, "Failed to build WinHttpHandler tuning delegate.");
                return _ => null;
            }
        }

        [DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WinHttpSetTimeouts(
            SafeHandle handle,
            int resolveTimeout,
            int connectTimeout,
            int sendTimeout,
            int receiveTimeout);
    }
}
