using NHibernate;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[RemoveMeWithToggle("merge with base type", Toggles.ResourcePlanner_ScheduleDeadlock_48170)]
	public class UnitOfWorkFactoryFactory48170 : UnitOfWorkFactoryFactory
	{
		public UnitOfWorkFactoryFactory48170(ICurrentPreCommitHooks currentPreCommitHooks, IEnversConfiguration enversConfiguration, ICurrentTransactionHooks transactionHooks, ICurrentHttpContext httpContext, IUpdatedBy updatedBy, ICurrentBusinessUnit businessUnit, INestedUnitOfWorkStrategy nestedUnitOfWorkStrategy) : base(currentPreCommitHooks, enversConfiguration, transactionHooks, httpContext, updatedBy, businessUnit, nestedUnitOfWorkStrategy)
		{
		}

		public override NHibernateUnitOfWorkFactory MakeAppFactory(ISessionFactory sessionFactory, string connectionString, string tenant)
		{
			return new NHibernateUnitOfWorkFactory(
				sessionFactory,
				_enversConfiguration.AuditSettingProvider,
				connectionString,
				_transactionHooks,
				tenant,
				_businessUnit,
				_nestedUnitOfWorkStrategy,
				this,
				true);
		}
	}
	
	public class UnitOfWorkFactoryFactory
	{
		private readonly ICurrentPreCommitHooks _currentPreCommitHooks;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_ScheduleDeadlock_48170)]
		protected readonly IEnversConfiguration _enversConfiguration;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_ScheduleDeadlock_48170)]
		protected readonly ICurrentTransactionHooks _transactionHooks;
		private readonly ICurrentHttpContext _httpContext;
		private readonly IUpdatedBy _updatedBy;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_ScheduleDeadlock_48170)]
		protected readonly ICurrentBusinessUnit _businessUnit;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_ScheduleDeadlock_48170)]
		protected readonly INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy;

		public UnitOfWorkFactoryFactory(
			ICurrentPreCommitHooks currentPreCommitHooks,
			IEnversConfiguration enversConfiguration,
			ICurrentTransactionHooks transactionHooks,
			ICurrentHttpContext httpContext,
			IUpdatedBy updatedBy,
			ICurrentBusinessUnit businessUnit,
			INestedUnitOfWorkStrategy nestedUnitOfWorkStrategy)
		{
			_currentPreCommitHooks = currentPreCommitHooks;
			_enversConfiguration = enversConfiguration;
			_transactionHooks = transactionHooks;
			_httpContext = httpContext;
			_updatedBy = updatedBy;
			_businessUnit = businessUnit;
			_nestedUnitOfWorkStrategy = nestedUnitOfWorkStrategy;
		}

		public NHibernateUnitOfWorkInterceptor MakeInterceptor()
		{
			return new NHibernateUnitOfWorkInterceptor(_updatedBy, _currentPreCommitHooks);
		}

		public virtual NHibernateUnitOfWorkFactory MakeAppFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant)
		{
			return new NHibernateUnitOfWorkFactory(
				sessionFactory,
				_enversConfiguration.AuditSettingProvider,
				connectionString,
				_transactionHooks,
				tenant,
				_businessUnit,
				_nestedUnitOfWorkStrategy,
				this,
				false);
		}

		public AnalyticsUnitOfWorkFactory MakeAnalyticsFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant)
		{
			return new AnalyticsUnitOfWorkFactory(
				sessionFactory,
				connectionString,
				tenant
			);
		}

		public ReadModelUnitOfWorkFactory MakeReadModelFactory(
			string applicationConnectionString
		)
		{
			var factory = new ReadModelUnitOfWorkFactory(_httpContext, applicationConnectionString);
			factory.Configure();
			return factory;
		}
	}
}