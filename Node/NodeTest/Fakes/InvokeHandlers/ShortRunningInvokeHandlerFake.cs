﻿using System;
using System.Threading;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
    internal class ShortRunningInvokeHandlerFake : IInvokeHandler
    {
        public void Invoke(object query,
                           CancellationTokenSource cancellationTokenSource,
                           Action<string> progressCallback)
        {
            progressCallback("waiting 2 seconds");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            progressCallback("done waiting");
        }
    }
}