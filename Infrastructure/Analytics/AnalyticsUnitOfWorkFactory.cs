using NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly TeleoptiUnitOfWorkContext _context;

		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString)
		{
			ConnectionString = connectionString;
			_context = new TeleoptiUnitOfWorkContext(sessionFactory);
			_factory = sessionFactory;
		}
		
		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			var session = _factory.OpenStatelessSession();
			return new NHibernateStatelessUnitOfWork(session);
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			var unitOfWork = _context.UnitOfWork;
			// maybe better to return null..
			// but mimic nhibernate session context for now
			if (unitOfWork == null)
				throw new HibernateException("No session bound to the current context");
			return unitOfWork;
		}

		public string ConnectionString { get; private set; }
		
		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			var session = _factory.OpenSession(new AggregateRootInterceptor());
			session.FlushMode = FlushMode.Never;
			new NHibernateUnitOfWork(
				_context,
				session,
				null,
				null,
				null,
				null,
				TransactionIsolationLevel.Default,
				null
				);
			return CurrentUnitOfWork();
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}