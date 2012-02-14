using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Core
{
    /// <summary>
    /// This thread pool is based on MSDN Magazine .Net Matters article February 2005 (Stephen Toub)
    /// It is a static thread pool which displays the beauty of the Semaphore construct.
    /// The Semaphore was a new construct to C# 2.0.
    /// </summary>
    public sealed class CustomThreadPool : ICustomThreadPool
    {
        // ReSharper disable InconsistentNaming
        private event EventHandler<UnhandledExceptionEventArgs> _unhandledException;
        // ReSharper restore InconsistentNaming
        private readonly Semaphore _workWaiting;
        private readonly Queue<WaitQueueItem> _queue;
        private List<Thread> _threads;
        private bool _disposed;

        public CustomThreadPool(int numberOfThreads, string name)
        {
            if (numberOfThreads <= 0) 
                throw new ArgumentOutOfRangeException("numberOfThreads", "Number of Threads must be greater than zero");
            _threads = new List<Thread>(numberOfThreads);
            _queue = new Queue<WaitQueueItem>();
            _workWaiting = new Semaphore(0, int.MaxValue);
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread t = new Thread(Run);
                t.IsBackground = true;
                t.Name = String.Format(CultureInfo.InvariantCulture, "{0}:{1}{2}", i, " ", name); 
                _threads.Add(t);
                t.Start();
            }
        }

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }
        }

        private void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            if (_unhandledException != null)
                _unhandledException(this, e);
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            if (_threads != null) // if (_threads == null) throw new ObjectDisposedException(GetType().Name);
            {
                if (callback == null) throw new ArgumentNullException("callback");

                WaitQueueItem item = new WaitQueueItem();
                item.Callback = callback;
                item.State = state;
                item.Context = ExecutionContext.Capture();

                lock (_queue) 
                    _queue.Enqueue(item);
                if (!_workWaiting.SafeWaitHandle.IsClosed)
                    _workWaiting.Release();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Run()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        _workWaiting.WaitOne();
                        WaitQueueItem item;
                        lock (_queue)
                            item = _queue.Dequeue();
                        ExecutionContext.Run(item.Context, new ContextCallback(item.Callback), item.State);
                    }
                    catch (ThreadInterruptedException)
                    {
                        return;
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            OnUnhandledException(new UnhandledExceptionEventArgs(ex, false));
                        }
                        catch (SocketException)
                        {
                        }
                    }
                }

            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        private class WaitQueueItem
        {
            public WaitCallback Callback;
            public object State;
            public ExecutionContext Context;
        }

        private void Dispose(bool isDisposing)
        {
            if (!_disposed)
            {
                if (isDisposing)
                {
                    if (_threads != null)
                    {
                        _threads.ForEach(t => t.Interrupt());
                        _threads = null;
                    }
                }
                if (_queue != null)
                    _queue.Clear();
                if (_workWaiting != null)
                    _workWaiting.Close();
                _disposed = true;
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CustomThreadPool()
        {
            Dispose(false);
        }

    }
}