using System;
using System.Threading;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
    internal class LongRunningInvokeHandlerFake : IInvokeHandler
    {
        public void Invoke(object query,
            CancellationTokenSource cancellationTokenSource, Action<string> progressCallback)
        {
            var someString = string.Empty;

            for (var i = 0; i < 200000; i++)
            {
                someString += "a";

                if (i%1000 == 0)
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
            }
        }
    }
}