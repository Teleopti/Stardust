using NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly UnitOfWorkContext _context;

		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString)
		{
			ConnectionString = connectionString;
			_context = new UnitOfWorkContext(sessionFactory);
			_factory = sessionFactory;
		}
		
		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			var session = _factory.OpenStatelessSession();
			return new NHibernateStatelessUnitOfWork(session);
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

		public string ConnectionString { get; private set; }
		
		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			new NHibernateUnitOfWork(
				_context,
				_factory.OpenSession(new AggregateRootInterceptor()),
				TransactionIsolationLevel.Default, 
				null);
			return CurrentUnitOfWork();
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}