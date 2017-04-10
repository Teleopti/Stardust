using System;
using System.Threading;

namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
    /// <summary>
    /// The result of an asynchronous operation.
    /// </summary>
    public class AsyncResult : IAsyncResult, IDisposable
    {
        private AsyncCallback _asyncCallback;
        private object _state;
        private ManualResetEventSlim _manualResetEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public AsyncResult(
            AsyncCallback callback,
            object state)
        {
            _asyncCallback = callback;
            _state = state;
            _manualResetEvent = new ManualResetEventSlim(false);
        }

        /// <summary>
        /// Completes this instance.
        /// </summary>
        public virtual void OnCompleted()
        {
            if(!_manualResetEvent.WaitHandle.SafeWaitHandle.IsClosed)
                _manualResetEvent.Set();
            if (_asyncCallback != null)
            {
                _asyncCallback(this);
            }
        }

        #region IAsyncResult Members
        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <value></value>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState
        {
            get
            {
                return _state;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return _manualResetEvent.WaitHandle;
            }
        }

        /// <summary>
        /// Gets an indication of whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <value></value>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an indication whether the asynchronous operation has completed.
        /// </summary>
        /// <value></value>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted
        {
            get
            {
                return _manualResetEvent.IsSet;
            }
        }
        #endregion IAsyncResult Members

        #region IDisposable Members
        private bool _isDisposed;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        [System.ComponentModel.Browsable(false)]
        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

        /// <summary>
        /// Occurs when this instance is disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _manualResetEvent.Dispose();
                    _manualResetEvent = null;
                    _state = null;
                    _asyncCallback = null;

                    EventHandler handler = Disposed;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                _isDisposed = true;
            }
        }

        /// <summary>
        ///    <para>
        ///        Checks if the instance has been disposed of, and if it has, throws an <see cref="ObjectDisposedException"/>; otherwise, does nothing.
        ///    </para>
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///    The instance has been disposed of.
        ///    </exception>
        ///    <remarks>
        ///    <para>
        ///        Derived classes should call this method at the start of all methods and properties that should not be accessed after a call to <see cref="Dispose()"/>.
        ///    </para>
        /// </remarks>
        protected void CheckDisposed()
        {
            if (_isDisposed)
            {
                string typeName = GetType().FullName;

                // TODO: You might want to move the message string into a resource file
                throw new ObjectDisposedException(
                    typeName,
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,
                                  "Cannot access a disposed {0}.",
                                  typeName));
            }
        }
        #endregion IDisposable Members
    }
}