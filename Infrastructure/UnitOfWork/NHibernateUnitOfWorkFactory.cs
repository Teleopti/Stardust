using System;
using NHibernate;
using NHibernate.Engine;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface INestedUnitOfWorkStrategy
	{
		void Strategize(ApplicationUnitOfWorkContext context);
	}

	public class SirLeakAlot : INestedUnitOfWorkStrategy
	{
		public void Strategize(ApplicationUnitOfWorkContext context)
		{
		}
	}

	public class ThrowOnNestedUnitOfWork : INestedUnitOfWorkStrategy
	{
		public void Strategize(ApplicationUnitOfWorkContext context)
		{
			if (context.Get() != null)
				throw new NestedUnitOfWorkException();
		}
	}

	public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly ApplicationUnitOfWorkContext _context;
		private readonly IAuditSetter _auditSettingProvider;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly IUpdatedBy _updatedBy;
		private readonly INestedUnitOfWorkStrategy _nestedUnitOfWork;

		protected internal NHibernateUnitOfWorkFactory(
			ISessionFactory sessionFactory, 
			IAuditSetter auditSettingProvider, 
			string connectionString, 
			ICurrentTransactionHooks transactionHooks, 
			IUpdatedBy updatedBy, 
			string tenant,
			INestedUnitOfWorkStrategy nestedUnitOfWork
			)
		{
			ConnectionString = connectionString;
			_factory = sessionFactory;
			_context = new ApplicationUnitOfWorkContext(tenant);
			_auditSettingProvider = auditSettingProvider;
			_transactionHooks = transactionHooks;
			_updatedBy = updatedBy;
			_nestedUnitOfWork = nestedUnitOfWork;
		}

		public string Name
		{
			get { return ((ISessionFactoryImplementor)_factory).Settings.SessionFactoryName; }
		}

		public ISessionFactory SessionFactory
		{
			get { return _factory; }
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			return new NHibernateStatelessUnitOfWork(_factory.OpenStatelessSession());
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			var unitOfWork = _context.Get();
			// maybe better to return null..
			// but mimic nhibernate session context for now
			if (unitOfWork == null)
				throw new HibernateException("No session bound to the current context");
			return unitOfWork;
		}

		public bool HasCurrentUnitOfWork()
		{
			return _context.Get() != null;
		}

		public IAuditSetter AuditSetting
		{
			get { return _auditSettingProvider; }
		}

		public string ConnectionString { get; private set; }

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return createAndOpenUnitOfWork(TransactionIsolationLevel.Default, QueryFilter.BusinessUnit);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel)
		{
			return createAndOpenUnitOfWork(isolationLevel, QueryFilter.BusinessUnit);
		}
		
		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return createAndOpenUnitOfWork(TransactionIsolationLevel.Default, businessUnitFilter);
		}

		private IUnitOfWork createAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel, IQueryFilter businessUnitFilter)
		{
			_nestedUnitOfWork.Strategize(_context);

			var session = _factory.OpenSession(new AggregateRootInterceptor(_updatedBy));

			businessUnitFilter.Enable(session, businessUnitId());
			QueryFilter.Deleted.Enable(session, null);
			QueryFilter.DeletedPeople.Enable(session, null);

			new NHibernateUnitOfWork(
				_context,
				session,
				isolationLevel,
				_transactionHooks);

			return CurrentUnitOfWork();
		}

		private static Guid businessUnitId()
		{
			return ServiceLocatorForEntity.CurrentBusinessUnit.Current() != null
				? ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault()
				: Guid.Empty;
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}

}
