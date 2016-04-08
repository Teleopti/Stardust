using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Proxy;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IsolationLevel = System.Data.IsolationLevel;
using TransactionException = NHibernate.TransactionException;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateUnitOfWork : IUnitOfWork
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateUnitOfWork));

		private readonly UnitOfWorkContext _context;
		private readonly ISession _session;
		private readonly TransactionIsolationLevel _isolationLevel;
		private readonly ICurrentPersistCallbacks _persistCallbacks;
		private readonly NHibernateFilterManager _filterManager;
		private readonly Lazy<AggregateRootInterceptor> _interceptor;
		private ITransaction _transaction;
		private IInitiatorIdentifier _initiator;
		private TransactionSynchronization _transactionSynchronization;

		protected internal NHibernateUnitOfWork(
			UnitOfWorkContext context, 
			ISession session, 
			TransactionIsolationLevel isolationLevel, 
			ICurrentPersistCallbacks persistCallbacks)
		{
			InParameter.NotNull("session", session);
			_context = context;
			_context.Set(this);
			_session = session;
			_isolationLevel = isolationLevel;
			_persistCallbacks = persistCallbacks ?? new NoPersistCallbacks();
			_filterManager = new NHibernateFilterManager(session);
			_interceptor = new Lazy<AggregateRootInterceptor>(() => (AggregateRootInterceptor) _session.GetSessionImplementation().Interceptor);
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
				_transactionSynchronization = new TransactionSynchronization(_persistCallbacks, _interceptor);
				_transaction.RegisterSynchronization(_transactionSynchronization);
			}
			catch (TransactionException transactionException)
			{
				throw new CouldNotCreateTransactionException("Cannot start transaction", transactionException);
			}
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

		public void Flush()
		{
			try
			{
				_interceptor.Value.Iteration = InterceptorIteration.Normal;
				Session.Flush();
				_interceptor.Value.Iteration = InterceptorIteration.UpdateRoots;
				blackSheep();
				Session.Flush();
			}
			catch (StaleStateException ex)
			{
				throw new OptimisticLockException("Optimistic lock", ex);
			}
		}

		private void blackSheep()
		{
			new SendPushMessageWhenRootAlteredService()
				.SendPushMessages(
					_interceptor.Value.ModifiedRoots,
					new PushMessagePersister(
						new PushMessageRepository(this),
						new PushMessageDialogueRepository(this),
						new CreatePushMessageDialoguesService()
						));
		}

		public IEnumerable<IRootChangeInfo> PersistAll()
		{
			return PersistAll(null);
		}

		public virtual IEnumerable<IRootChangeInfo> PersistAll(IInitiatorIdentifier initiator)
		{
			_initiator = initiator;

			//man borde nog styra upp denna genom att använda ISynchronization istället,
			//när tran startas, lägg pĺ en sync callback via tran.RegisterSynchronization(callback);
			ICollection<IRootChangeInfo> modifiedRoots;
			try
			{
				Flush();

				modifiedRoots = new List<IRootChangeInfo>(_interceptor.Value.ModifiedRoots);
				
				commitInnerTransaction();
				
				mightStartTransaction();
				_persistCallbacks.Current().ForEach(d => d.AfterFlush(modifiedRoots));
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
				_interceptor.Value.Clear();
			}
			return modifiedRoots;
		}

		private void commitInnerTransaction()
		{
			_transaction.Commit();
			_transaction.Dispose();
			_transaction = null;
			_transactionSynchronization = null;
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

		public void AfterSuccessfulTx(Action func)
		{
			mightStartTransaction();
			_transactionSynchronization.RegisterForAfterCompletion(func);
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
		
		public void Dispose()
		{
			if (_context != null)
				_context.Clear();

			if (_session != null)
			{
				safeRollback();
				_session.Dispose();
			}

			if (_interceptor.IsValueCreated)
				_interceptor.Value.Clear();
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
