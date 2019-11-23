using System;
using System.Threading;

namespace Node.Interfaces
{
    public interface IInvokeHandler
    {
        void Invoke(object query,
            CancellationTokenSource cancellationTokenSource, Action<string> progressCallback);
    }
}