using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Proxy;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IsolationLevel = System.Data.IsolationLevel;
using TransactionException = NHibernate.TransactionException;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public abstract class NHibernateUnitOfWork : IUnitOfWork
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateUnitOfWork));

		private readonly UnitOfWorkContext _context;
		private readonly ISession _session;
		private readonly TransactionIsolationLevel _isolationLevel;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly NHibernateFilterManager _filterManager;
		protected readonly Lazy<AggregateRootInterceptor> Interceptor;
		private ITransaction _transaction;
		private IInitiatorIdentifier _initiator;
		private TransactionSynchronization _transactionSynchronization;

		protected internal NHibernateUnitOfWork(
			UnitOfWorkContext context, 
			ISession session, 
			TransactionIsolationLevel isolationLevel, 
			ICurrentTransactionHooks transactionHooks)
		{
			InParameter.NotNull("session", session);
			_context = context;
			_context.Set(this);
			_session = session;
			_session.FlushMode = FlushMode.Never;
			_isolationLevel = isolationLevel;
			_transactionHooks = transactionHooks ?? new NoTransactionHooks();
			_filterManager = new NHibernateFilterManager(session);
			Interceptor = new Lazy<AggregateRootInterceptor>(() => (AggregateRootInterceptor) _session.GetSessionImplementation().Interceptor);
		}
		
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
			return _filterManager.Disable(filter);
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
				Interceptor.Value.Iteration = InterceptorIteration.Normal;
				Session.Flush();
				Interceptor.Value.Iteration = InterceptorIteration.UpdateRoots;
				BlackSheep();	// <-----
				Session.Flush();
			}
			catch (StaleStateException ex)
			{
				throw new OptimisticLockException("Optimistic lock", ex);
			}
		}

		protected virtual void BlackSheep()
		{
		}

		public void AfterSuccessfulTx(Action func)
		{
			transactionEnsure();
			_transactionSynchronization.RegisterForAfterCompletion(func);
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return PersistAll(null);
		}

		public virtual IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			_initiator = initiator;

			IEnumerable<IRootChangeInfo> modifiedRoots;
			try
			{
				Flush();
				modifiedRoots = Interceptor.Value.ModifiedRoots.ToList();
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
			finally
			{
				Interceptor.Value.Clear();
			}
			return modifiedRoots;
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
				_transaction = _isolationLevel == TransactionIsolationLevel.Default ?
					_session.BeginTransaction() :
					_session.BeginTransaction(IsolationLevel.Serializable);
			}
			catch (TransactionException transactionException)
			{
				throw new CouldNotCreateTransactionException("Cannot start transaction", transactionException);
			}

			_transactionSynchronization = new TransactionSynchronization(_transactionHooks, Interceptor);
			_transaction.RegisterSynchronization(_transactionSynchronization);
		}

		private void transactionCommit()
		{
			_transaction.Commit();
			var exception = _transactionSynchronization.Exception;
			_transaction.Dispose();
			_transaction = null;
			_transactionSynchronization = null;
			if (exception != null)
				throw exception;
		}

		private void transactionRollback()
		{
			if (_transaction == null || !_transaction.IsActive) return;

			try
			{
				_transaction.Rollback();
				_transaction.Dispose();
				_transaction = null;
				_transactionSynchronization = null;
			}
			catch (Exception ex)
			{
				_logger.Error("Cannot rollback transaction! " + ex);
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

		public int? DatabaseVersion(IAggregateRoot root, bool usePessimisticLock=false)
		{
			if (!root.Id.HasValue)
				throw new ArgumentException("Cannot find " + root + " in db");
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

		public void Dispose()
		{
			if (_context != null)
				_context.Clear();

			if (_session != null)
			{
				transactionRollback();
				_session.Dispose();
			}

			if (Interceptor.IsValueCreated)
				Interceptor.Value.Clear();
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
