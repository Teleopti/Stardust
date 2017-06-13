using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly AnalyticsUnitOfWorkContext _context;

		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant,
			ICurrentTransactionHooks transactionHooks
			)
		{
			ConnectionString = connectionString;
			_factory = sessionFactory;
			_transactionHooks = transactionHooks;
			_context = new AnalyticsUnitOfWorkContext(tenant);
		}
		
		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			var session = _factory.OpenStatelessSession();
			return new NHibernateStatelessUnitOfWork(session);
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			return _context.Get();
		}

		public string ConnectionString { get; }
		
		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			new AnalyticsUnitOfWork(
				_context,
				_factory.OpenSession(new AggregateRootInterceptor(ServiceLocatorForLegacy.UpdatedBy, new NoPreCommitHooks())),
				_transactionHooks
				);

			return CurrentUnitOfWork();
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}