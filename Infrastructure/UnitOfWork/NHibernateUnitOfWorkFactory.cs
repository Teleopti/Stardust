using System;
using System.Threading;
using NHibernate;
using NHibernate.Engine;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly UnitOfWorkContext _context;
		private readonly IAuditSetter _auditSettingProvider;
		private readonly ICurrentPersistCallbacks _persistCallbacks;

		protected internal NHibernateUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			IAuditSetter auditSettingProvider,
			string connectionString,
			ICurrentPersistCallbacks persistCallbacks)
		{
			ConnectionString = connectionString;
			_context = new UnitOfWorkContext(sessionFactory);
			_factory = sessionFactory;
			_auditSettingProvider = auditSettingProvider;
			_persistCallbacks = persistCallbacks;
		}

		public string Name
		{
			get { return ((ISessionFactoryImplementor)_factory).Settings.SessionFactoryName; }
		}

		public ISessionFactory SessionFactory
		{
			get { return _factory; }
		}

		public long? NumberOfLiveUnitOfWorks
		{
			get
			{
				var statistics = _factory.Statistics;
				if(statistics.IsStatisticsEnabled)
					return statistics.SessionOpenCount - statistics.SessionCloseCount;
				return null;
			}
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

		public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default)
		{
			return createAndOpenUnitOfWork(isolationLevel, QueryFilter.BusinessUnit);
		}
		
		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return createAndOpenUnitOfWork(TransactionIsolationLevel.Default, businessUnitFilter);
		}

		private IUnitOfWork createAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel, IQueryFilter businessUnitFilter)
		{
			var businessUnitId = getBusinessUnitId();
			var session = _factory.OpenSession(new AggregateRootInterceptor());

			businessUnitFilter.Enable(session, businessUnitId);
			QueryFilter.Deleted.Enable(session, null);
			QueryFilter.DeletedPeople.Enable(session, null);

			new NHibernateUnitOfWork(
				_context,
				session,
				isolationLevel,
				_persistCallbacks);

			return CurrentUnitOfWork();
		}

		private static Guid getBusinessUnitId()
		{
			var identity = Thread.CurrentPrincipal.Identity as ITeleoptiIdentity;
			var buId = (identity != null && identity.BusinessUnit != null)
				? identity.BusinessUnit.Id.GetValueOrDefault()
				: Guid.Empty;
			return buId;
		}
		
		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}
