using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NHibernate;
using NHibernate.Transaction;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NestedAnalyticsUnitOfWorkException : Exception
	{
	}

	public class AnalyticsUnitOfWork : IUnitOfWork, IHaveSession
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(AnalyticsUnitOfWork));

		private readonly AnalyticsUnitOfWorkContext _context;
		private readonly ISession _session;
		private ITransaction _transaction;
		private IInitiatorIdentifier _initiator;

		protected internal AnalyticsUnitOfWork(AnalyticsUnitOfWorkContext context, ISession session)
		{
			if (context.Get() != null)
				throw new NestedAnalyticsUnitOfWorkException();
			_context = context;
			_context.Set(this);
			_session = session;
			_session.FlushMode = FlushMode.Manual;
		}

		public ISession GetSession() => Session;

		protected internal virtual ISession Session
		{
			get
			{
				transactionEnsure();
				return _session;
			}
		}

		public IDisposable DisableFilter(IQueryFilter filter)
		{
			throw new NotImplementedException();
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

		public void Flush()
		{
			try
			{
				Session.Flush();
			}
			catch (StaleStateException ex)
			{
				throw new OptimisticLockException("Optimistic lock", ex);
			}
		}

		public void AfterSuccessfulTx(Action action)
		{
			transactionEnsure();
			_transaction.RegisterSynchronization(new transactionCallback(action));
		}

		private class transactionCallback : ITransactionCompletionSynchronization
		{
			private readonly Action _callback;

			public transactionCallback(Action callback)
			{
				_callback = callback;
			}

			public void ExecuteBeforeTransactionCompletion()
			{
			}

			public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}

			public void ExecuteAfterTransactionCompletion(bool success)
			{
				if (success)
				{
					_callback();
				}
			}

			public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return PersistAll(null);
		}

		public virtual IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			_initiator = initiator;

			try
			{
				Flush();
				transactionCommit();
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

			return null;
		}

		private void persistExceptionHandler(Exception ex)
		{
			_logger.Error("An error occurred when trying to save.", ex);
			transactionRollback();
			if (_session.IsOpen)
				_session.Close();
		}

		private void transactionEnsure()
		{
			if (_transaction != null) return;

			try
			{
				_transaction = _session.BeginTransaction();
			}
			catch (TransactionException transactionException)
			{
				throw new CouldNotCreateTransactionException("Cannot start transaction", transactionException);
			}
		}

		private void transactionCommit()
		{
			_transaction.Commit();
			_transaction.Dispose();
			_transaction = null;
		}

		private void transactionRollback()
		{
			if (_transaction == null || !_transaction.IsActive) return;

			var transaction = _transaction;
			_transaction = null;
			try
			{
				transaction.Rollback();
				transaction.Dispose();
			}
			catch (Exception ex)
			{
				_logger.Error("Cannot rollback transaction!", ex);
				//don't do anything - should be handled higher up the chain
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

		public void Dispose()
		{
			if (_context != null)
				_context.Clear();

			if (_session != null)
			{
				transactionRollback();
				_session.Dispose();
			}
		}

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