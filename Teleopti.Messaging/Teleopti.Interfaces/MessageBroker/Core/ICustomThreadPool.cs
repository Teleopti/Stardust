using System;
using System.Threading;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The custom threadpool.
    /// </summary>
    public interface ICustomThreadPool : IDisposable
    {
        /// <summary>
        /// Occurs when [unhandled exception].
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
        /// <summary>
        /// Queues the user work item.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        void QueueUserWorkItem(WaitCallback callback, object state);
    }
}