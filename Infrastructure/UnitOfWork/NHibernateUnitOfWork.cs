using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Proxy;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IsolationLevel = System.Data.IsolationLevel;
using TransactionException = NHibernate.TransactionException;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateUnitOfWork : IUnitOfWork
	{
		private Lazy<AggregateRootInterceptor> _interceptor;
		private readonly TeleoptiUnitOfWorkContext _context;
		private ISession _session;
		private IMessageBrokerComposite _messageBroker;
		private bool disposed;
		private ITransaction _transaction;
		private readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateUnitOfWork));
		private NHibernateFilterManager _filterManager;
		private ICurrentPersistCallbacks _persistCallbacks;
		private readonly TransactionIsolationLevel _isolationLevel;
		private ISendPushMessageWhenRootAlteredService _sendPushMessageWhenRootAlteredService;
		private IInitiatorIdentifier _initiator;

		protected internal NHibernateUnitOfWork(
			TeleoptiUnitOfWorkContext context,
			ISession session, 
			IMessageBrokerComposite messageBroker, 
			ICurrentPersistCallbacks persistCallbacks, 
			NHibernateFilterManager filterManager, 
			ISendPushMessageWhenRootAlteredService sendPushMessageWhenRootAlteredService, 
			TransactionIsolationLevel isolationLevel, 
			IInitiatorIdentifier initiator)
		{
			InParameter.NotNull("session", session);
			_context = context;
			if (_context != null)
				_context.UnitOfWork = this;
			_session = session;
			_messageBroker = messageBroker;
			_filterManager = filterManager;
			_sendPushMessageWhenRootAlteredService = sendPushMessageWhenRootAlteredService;
			_isolationLevel = isolationLevel;
			setInitiator(initiator);
			_persistCallbacks = persistCallbacks;
			_interceptor = new Lazy<AggregateRootInterceptor>(() => (AggregateRootInterceptor) _session.GetSessionImplementation().Interceptor);
		}

		protected internal AggregateRootInterceptor Interceptor
		{
			get
			{
				return _interceptor.Value;
			}
		}

		protected internal virtual ISession Session
		{
			get
			{
				mightStartTransaction();
				return _session;
			}
		}

		private void mightStartTransaction()
		{
			if (_transaction != null) return;
			try
			{
				_transaction = _isolationLevel == TransactionIsolationLevel.Default ?
					_session.BeginTransaction() :
					_session.BeginTransaction(IsolationLevel.Serializable);
			}
			catch (TransactionException transactionException)
			{
				throw new CouldNotCreateTransactionException("Cannot start transaction", transactionException);
			}
		}

		private void setInitiator(IInitiatorIdentifier initiator)
		{
			_initiator = initiator;
		}

		public IInitiatorIdentifier Initiator()
		{
			return _initiator;
		}

		public void Clear()
		{
			Session.Clear();
		}

		public T Merge<T>(T root) where T : class, IAggregateRoot
		{
			if (root.Id == null)
				throw new DataSourceException("Cannot merge transient root.");
			try
			{
				return Session.Merge(root);
			}
			catch (StaleStateException staleStateEx)
			{
				throw new OptimisticLockException("Optimistic lock", staleStateEx);
			}
		}

		public bool Contains(IEntity entity)
		{
			return Session.Contains(entity);
		}

		public bool IsDirty()
		{
			return Session.IsDirty();
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return PersistAll(null);
		}


		public virtual IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			// next step: move to only use constructor injection.
			setInitiator(initiator);

			//man borde nog styra upp denna genom att använda ISynchronization istället,
			//när tran startas, lägg pĺ en sync callback via tran.RegisterSynchronization(callback);
			ICollection<IRootChangeInfo> modifiedRoots;
			try
			{
				Flush();

				modifiedRoots = new List<IRootChangeInfo>(Interceptor.ModifiedRoots);
				
				commitInnerTransaction();
				
				mightStartTransaction();
				invokeCallbacks(modifiedRoots);
				commitInnerTransaction();
			}
			catch (TooManyActiveAgentsException exception)
			{
				persistExceptionHandler(exception);
				throw;
			}
			catch (DataSourceException ex)
			{
				persistExceptionHandler(ex);
				throw;
			}
			catch (Exception ex)
			{
				persistExceptionHandler(ex);
				PreserveStack.ForInnerOf(ex);
				throw new DataSourceException("Cannot commit transaction! ", ex);
			}
			finally
			{
				Interceptor.Clear();
			}
			notifyBroker(initiator, modifiedRoots);
			return modifiedRoots;
		}

		private void commitInnerTransaction()
		{
			_transaction.Commit();
			_transaction.Dispose();
			_transaction = null;
		}

		private void invokeCallbacks(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			if (_persistCallbacks == null) return;

			_persistCallbacks.Current().ForEach(d =>
				{
					using (PerformanceOutput.ForOperation(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Sending message with {0}", d.GetType())))
					{
						d.AfterFlush(modifiedRoots);
					}
				});
		}

		private void notifyBroker(IInitiatorIdentifier identifier, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			Guid moduleId = identifier == null ? Guid.Empty : identifier.InitiatorId;
			new NotifyMessageBroker(_messageBroker).Notify(moduleId, modifiedRoots);
		}

		private void persistExceptionHandler(Exception ex)
		{
			_logger.Error("An error occurred when trying to save.", ex);
			safeRollback();
			if (_session.IsOpen)
				_session.Close();
		}

		public void Reassociate(IAggregateRoot root)
		{
			Session.Lock(root, LockMode.None);
		}


		public void Reassociate<T>(params IEnumerable<T>[] rootCollectionsCollection) where T : IAggregateRoot
		{
			rootCollectionsCollection.ForEach(coll => coll.ForEach(r => Reassociate(r)));
		}

		public void Refresh(IAggregateRoot root)
		{
			Session.Refresh(root);
		}

		public void Remove(IAggregateRoot root)
		{
			Session.Evict(root);
		}

		public int? DatabaseVersion(IAggregateRoot root, bool usePessimisticLock=false)
		{
			if (!root.Id.HasValue)
				throwIncorrectDbVersionParameter(root);
			int result;
			
			var proxy = root as INHibernateProxy;
			var type = proxy == null ? root.GetType() : proxy.HibernateLazyInitializer.PersistentClass;
			if (usePessimisticLock)
			{
				// use hql query here when SetLockMode works on non entities. Seems to be a problem currently
				//possible sql injection here but that's pretty lĺngsökt so never mind...
				var sql = "select Version from " + type.Name + " with(updlock, holdlock) where Id =:id";
				result = Session.CreateSQLQuery(sql)
					.SetGuid("id", root.Id.Value)
					.UniqueResult<int>();
			}
			else
			{
				result = Session.CreateCriteria(type)
					.Add(Restrictions.Eq("Id", root.Id))
					.SetProjection(Projections.Property("Version"))
					.UniqueResult<int>();				
			}
			if (result == 0)
				return null;
			return result;
		}

		public IDisposable DisableFilter(IQueryFilter filter)
		{
			return _filterManager.Disable(filter);
		}

		public void Flush()
		{
			try
			{
				Interceptor.Iteration = InterceptorIteration.Normal;
				Session.Flush();
				Interceptor.Iteration = InterceptorIteration.UpdateRoots;
				if (_sendPushMessageWhenRootAlteredService != null)
					_sendPushMessageWhenRootAlteredService.SendPushMessages(Interceptor.ModifiedRoots,
						new PushMessagePersister(new PushMessageRepository(this), new PushMessageDialogueRepository(this),
							new CreatePushMessageDialoguesService()));
				Session.Flush();
			}
			catch (StaleStateException ex)
			{
				throw new OptimisticLockException("Optimistic lock", ex);
			}
		}

		//right now only supporting one callback. if you call this twice - the latter will overwrite the first one
		public void AfterSuccessfulTx(Action func)
		{
			Session.Transaction.RegisterSynchronization(new TransactionCallback(func));
		}

		private void safeRollback()
		{
			if (_transaction != null && _transaction.IsActive)
			{
				try
				{
					_transaction.Rollback();
					_transaction.Dispose();
				}
				catch (Exception ex)
				{
					_logger.Error("Cannot rollback transaction! " + ex);
					//don't do anything - should be handled higher up the chain
				}
			}
		}

		private static void throwIncorrectDbVersionParameter(IAggregateRoot root)
		{
			throw new ArgumentException("Cannot find " + root + " in db");
		}

		#region IDispose

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
			if (_context != null)
				_context.UnitOfWork = null;

			if (_session != null)
			{
				safeRollback();

				_session.Dispose();
				_session = null;

				_filterManager = null;

				_persistCallbacks = null;
				_messageBroker = null;
				_sendPushMessageWhenRootAlteredService = null;
			}

			if (_interceptor.IsValueCreated)
			{
				_interceptor.Value.Clear();
				_interceptor = null;
			}
		}

		#endregion

		public override bool Equals(object obj)
		{
			var uow = obj as NHibernateUnitOfWork;
			return uow != null && Session == uow.Session;
		}

		public override int GetHashCode()
		{
			return Session.GetHashCode();
		}
	}
}
