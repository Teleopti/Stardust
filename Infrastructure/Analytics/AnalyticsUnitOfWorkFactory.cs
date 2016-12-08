using NHibernate;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly IUpdatedBy _updatedBy;
		private readonly AnalyticsUnitOfWorkContext _context;


		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant,
			ICurrentTransactionHooks transactionHooks,
			IUpdatedBy updatedBy)
		{
			ConnectionString = connectionString;
			_factory = sessionFactory;
			_transactionHooks = transactionHooks;
			_updatedBy = updatedBy;
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
				_factory.OpenSession(new AggregateRootInterceptor(_updatedBy)),
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