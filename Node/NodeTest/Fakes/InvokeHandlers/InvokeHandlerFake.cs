using System;
using System.Threading;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
    internal class InvokeHandlerFake : IInvokeHandler
    {
        public void Invoke(object query,
            CancellationTokenSource cancellationTokenSource, Action<string> progressCallback)
        {
        }
    }
}