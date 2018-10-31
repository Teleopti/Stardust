using NHibernate;
using NHibernate.Engine;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly ApplicationUnitOfWorkContext _context;
		private readonly IAuditSetter _auditSettingProvider;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy;
		private readonly UnitOfWorkFactoryFactory _unitOfWorkFactoryFactory;

		// can not inject here because the lifetime of the factory
		// is longer than the container when running unit tests
		protected internal NHibernateUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			IAuditSetter auditSettingProvider,
			string connectionString,
			ICurrentTransactionHooks transactionHooks,
			string tenant,
			ICurrentBusinessUnit businessUnit,
			INestedUnitOfWorkStrategy nestedUnitOfWorkStrategy, 
			UnitOfWorkFactoryFactory unitOfWorkFactoryFactory)
		{
			ConnectionString = connectionString;
			_factory = sessionFactory;
			_context = new ApplicationUnitOfWorkContext(tenant);
			_auditSettingProvider = auditSettingProvider;
			_transactionHooks = transactionHooks;
			_businessUnit = businessUnit;
			_nestedUnitOfWorkStrategy = nestedUnitOfWorkStrategy;
			_unitOfWorkFactoryFactory = unitOfWorkFactoryFactory;
		}

		public string Name => ((ISessionFactoryImplementor) _factory).Settings.SessionFactoryName;

		public ISessionFactory SessionFactory => _factory;

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

		public IAuditSetter AuditSetting => _auditSettingProvider;

		public string ConnectionString { get; }

		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			return CreateAndOpenUnitOfWork(QueryFilter.BusinessUnit);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			_nestedUnitOfWorkStrategy.Strategize(_context);

			var interceptor = _unitOfWorkFactoryFactory.MakeInterceptor();
			var session = _factory
				.WithOptions()
				.Interceptor(interceptor)
				.OpenSession();

			businessUnitFilter.Enable(session, _businessUnit.CurrentId().GetValueOrDefault());
			QueryFilter.Deleted.Enable(session, null);
			QueryFilter.DeletedPeople.Enable(session, null);

			var unitOfWork = new NHibernateUnitOfWork(
				_context.Clear,
				session,
				interceptor,
				_transactionHooks);
			_context.Set(unitOfWork);

			return unitOfWork;
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}