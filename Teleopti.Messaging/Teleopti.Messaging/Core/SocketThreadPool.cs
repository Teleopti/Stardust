using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Core
{
    /// <summary>
    /// This thread pool is based on MSDN Magazine .Net Matters article February 2005 (Stephen Toub)
    /// It is a static thread pool which displays the beauty of the Semaphore construct.
    /// The Semaphore was a new construct to C# 2.0.
    /// </summary>
    public sealed class SocketThreadPool : IDisposable
    {
        public const string SocketInfoConst = "SocketInfo";
        private const string ExceptionMessageConst = "Socket information is null";
        private event EventHandler<UnhandledExceptionEventArgs> unhandledException;
        private readonly Semaphore _workWaiting;
        private readonly Queue<WaitQueueItem> _queue;
        private List<Thread> _threads;

        [ThreadStatic] private ISocketInfo _socketInfo;

        public SocketThreadPool(string name, IList<ISocketInfo> socketInformation)
        {
            if (socketInformation.Count <= 0)
                throw new ArgumentOutOfRangeException("socketInformation", "Number of ports must be greater than zero");
            _threads = new List<Thread>(socketInformation.Count);
            _queue = new Queue<WaitQueueItem>();
            _workWaiting = new Semaphore(0, int.MaxValue);
            for (int i = 0; i < socketInformation.Count; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Run));
                t.IsBackground = true;
                t.Name = String.Format(CultureInfo.CurrentCulture, "{0}:{1}{2}", i, " ", name);
                _threads.Add(t);
                if(socketInformation[i] == null)
                    throw new SocketIsNullException(ExceptionMessageConst);
                t.Start(socketInformation[i]);
            }
        }

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException
        {
            add { unhandledException += value; }
            remove { unhandledException -= value; }
        }

        private void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            if (unhandledException != null)
                unhandledException(this, e);
        }

        public ISocketInfo SocketInfo
        {
            get { return _socketInfo;  }
        }

        public void QueueUserWorkItem(WaitCallback callback, object state)
        {
            //if (_threads == null) throw new ObjectDisposedException(GetType().Name);

            if (_threads != null) 
            {
                if (callback == null) throw new ArgumentNullException("callback");

                WaitQueueItem item = new WaitQueueItem();
                item.Callback = callback;
                item.State = state;
                item.Context = ExecutionContext.Capture();

                lock (_queue) _queue.Enqueue(item);
                _workWaiting.Release();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Run(object state)
        {
            try
            {

                InitialiseThreadStaticAttribute(state);

                while (true)
                {
                    try
                    {
                        _workWaiting.WaitOne();
                        WaitQueueItem item;
                        lock (_queue) item = _queue.Dequeue();
                        ExecutionContext.Run(item.Context, new ContextCallback(item.Callback), item.State);
                    }
                    catch (ThreadInterruptedException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        OnUnhandledException(new UnhandledExceptionEventArgs(ex, false));
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void InitialiseThreadStaticAttribute(object state)
        {
            _socketInfo = (ISocketInfo) state;
        }

        private class WaitQueueItem
        {
            public WaitCallback Callback;
            public object State;
            public ExecutionContext Context;
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_threads != null)
                {
                    _threads.ForEach(delegate(Thread t) { t.Interrupt(); });
                    _threads = null;
                }
            }
            if (_workWaiting != null)
                _workWaiting.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SocketThreadPool()
        {
            Dispose(false);
        }

    }

}