using System;
using System.Threading;

namespace Node.Interfaces
{
    public interface IHandle<T>
    {
        void Handle(T parameters,
            CancellationTokenSource cancellationTokenSource, Action<string> sendProgress);
    }
}