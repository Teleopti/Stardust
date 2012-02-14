using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Interfaces.Core;
using Teleopti.Messaging.Interfaces.Events;

namespace Teleopti.Messaging.Core
{
    /// <summary>
    /// Represents a cache scavenger that runs on a background thread.
    /// </summary>
    public class BackgroundQueueReader : IBackgroundQueueReader
    {
        private event EventHandler<UnhandledExceptionEventArgs> _errorEvent;
        private readonly ProducerConsumer _inputQueue = new ProducerConsumer();
        private Thread _inputQueueThread;
        private bool _isActive;
        private bool _running;

        public void Start(string name)
        {
            ThreadStart queueReader = new ThreadStart(StartThread);
            _inputQueueThread = new Thread(queueReader);
            _inputQueueThread.IsBackground = true;
            _inputQueueThread.Name = name;
            _inputQueueThread.Start();
        }

        public void Start(string name, EventHandler anonmousDelegate)
        {
            ParameterizedThreadStart queueReader = new ParameterizedThreadStart(StartThread);
            _inputQueueThread = new Thread(queueReader);
            _inputQueueThread.IsBackground = true;
            _inputQueueThread.Name = name;
            _inputQueueThread.Start(anonmousDelegate);
        }

        public event EventHandler<UnhandledExceptionEventArgs> ErrorEvent
        {
            add { _errorEvent += value; }
            remove { _errorEvent -= value; }
        }

        /// <summary>
        /// Starts the scavenger.
        /// </summary>
        private void StartThread(object state)
        {
            _running = true;

            EventHandler anonmousDelegate = (EventHandler) state;
            anonmousDelegate.Invoke(this, EventArgs.Empty);
            
            QueueReader();
        }

        /// <summary>
        /// Starts the scavenger.
        /// </summary>
        private void StartThread()
        {
            _running = true;
            QueueReader();
        }

        /// <summary>
        /// Stops the scavenger.
        /// </summary>
        public void StopReading()
        {
            _running = false;
            _inputQueueThread.Interrupt();
        }

        /// <summary>
        /// Determines if the queue reader is active.
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
        }
		
        public void Enqueue(IQueueMessage queueItem)
        {
            _inputQueue.Enqueue(queueItem);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void QueueReader()
        {
            _isActive = true;
            while (_running)
            {
                IQueueMessage msg = _inputQueue.Dequeue() as IQueueMessage;
                try
                {
                    if (msg == null)
                        continue;
                    msg.Run();
                }
                catch (Exception exc)
                {
                    if(_errorEvent != null)
                        _errorEvent(this, new UnhandledExceptionEventArgs(exc, false));
                }
            }
            _isActive = false;
        }


        public void Enqueue(WaitCallback callback, byte[] package)
        {
            Enqueue(new QueueMessage(callback, package));    
        }

        protected virtual void Dispose(bool isDisposable)
        {
            if(isDisposable)
                StopReading();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    public class QueueMessage : IQueueMessage
    {
        private readonly WaitCallback _callback;
        private readonly object _package;

        public QueueMessage(WaitCallback callback, object package)
        {
            _callback = callback;
            _package = package;
        }

        public void Run()
        {
            _callback.Invoke(_package);
        }
    }

}