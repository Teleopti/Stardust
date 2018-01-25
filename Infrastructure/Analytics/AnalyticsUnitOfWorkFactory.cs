using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly AnalyticsUnitOfWorkContext _context;

		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant
			)
		{
			ConnectionString = connectionString;
			_factory = sessionFactory;
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
				_factory.OpenSession()
				);

			return CurrentUnitOfWork();
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}