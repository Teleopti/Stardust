using System;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    /// <summary>
    /// An UnitOfWork implementation built as
    /// a wrapper around nhibernate's stateless session
    /// </summary>
    public class NHibernateStatelessUnitOfWork : IStatelessUnitOfWork
    {
    	private readonly IStatelessSession _session;
        private bool disposed;

    	/// <summary>
        /// Initializes a new instance of the <see cref="NHibernateStatelessUnitOfWork"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-22
        /// </remarks>
        protected internal NHibernateStatelessUnitOfWork(IStatelessSession session)
        {
            InParameter.NotNull(nameof(session), session);
            _session = session;
        }

    	/// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
		protected internal IStatelessSession Session => _session;

	    ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        /// <remarks>
        /// So far only managed code. No need implementing destructor.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            //Don't know how to test next row. Impossible?
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ReleaseManagedResources();
                }
                ReleaseUnmanagedResources();
                disposed = true;                
            }
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
	        _session?.Dispose();
        }
    }
}
