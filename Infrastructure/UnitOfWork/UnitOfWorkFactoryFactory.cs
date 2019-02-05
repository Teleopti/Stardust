using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class UnitOfWorkFactoryFactory
	{
		private readonly ICurrentPreCommitHooks _currentPreCommitHooks;
		private readonly ICurrentTransactionHooks _transactionHooks;
		private readonly ICurrentHttpContext _httpContext;
		private readonly IUpdatedBy _updatedBy;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy;

		public UnitOfWorkFactoryFactory(
			ICurrentPreCommitHooks currentPreCommitHooks,
			ICurrentTransactionHooks transactionHooks,
			ICurrentHttpContext httpContext,
			IUpdatedBy updatedBy,
			ICurrentBusinessUnit businessUnit,
			INestedUnitOfWorkStrategy nestedUnitOfWorkStrategy)
		{
			_currentPreCommitHooks = currentPreCommitHooks;
			_transactionHooks = transactionHooks;
			_httpContext = httpContext;
			_updatedBy = updatedBy;
			_businessUnit = businessUnit;
			_nestedUnitOfWorkStrategy = nestedUnitOfWorkStrategy;
		}

		public NHibernateUnitOfWorkInterceptor MakeInterceptor()
		{
			return new NHibernateUnitOfWorkInterceptor(_updatedBy, _businessUnit, _currentPreCommitHooks);
		}

		public virtual NHibernateUnitOfWorkFactory MakeAppFactory(
			ISessionFactory sessionFactory,
			string connectionString,
			string tenant)
		{
			return new NHibernateUnitOfWorkFactory(
				sessionFactory,
				connectionString,
				_transactionHooks,
				tenant,
				_businessUnit,
				_nestedUnitOfWorkStrategy,
				this);
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