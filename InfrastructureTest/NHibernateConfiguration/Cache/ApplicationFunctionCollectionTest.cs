using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration.Cache
{
	[TestFixture]
	[Category("BucketB")]
	public class ApplicationFunctionCollectionTest : DatabaseTestWithoutTransaction
	{
		private IDataSource dataSource;
		private IApplicationRole applicationRole;
		private IApplicationFunction applicationFunction;

		[Test]
		public void ShouldCacheApplicationRoleFunctionCollection()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new ApplicationRoleRepository(uow).Get(applicationRole.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(p.ApplicationFunctionCollection);
			}
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldCacheApplicationFunction()
		{
			var sessionFactory = ((NHibernateUnitOfWorkFactory)dataSource.Application).SessionFactory;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var p = new ApplicationRoleRepository(uow).Get(applicationRole.Id.Value);
				sessionFactory.Statistics.Clear();
				LazyLoadingManager.Initialize(p.ApplicationFunctionCollection);
			}
			sessionFactory.Statistics.CollectionLoadCount.Should().Be.EqualTo(0);
		}


		[SetUp]
		public void Setup1()
		{
			var dsFactory = new DataSourcesFactory(new EnversConfiguration(), new NoTransactionHooks(), DataSourceConfigurationSetter.ForTestWithCache(), new CurrentHttpContext(), new NoNhibernateConfigurationCache(), new NoPreCommitHooks());
			dataSource = dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(InfraTestConfigReader.ConnectionString, null), null);
			applicationFunction = new ApplicationFunction();
			applicationRole = new ApplicationRole { Name = "hejhej" };
			applicationRole.AddApplicationFunction(applicationFunction);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new ApplicationFunctionRepository(uow).Add(applicationFunction);
				new ApplicationRoleRepository(uow).Add(applicationRole);
				uow.PersistAll();
			}

			//fill cache
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var role = new ApplicationRoleRepository(uow).Get(applicationRole.Id.Value);
				LazyLoadingManager.Initialize(role.ApplicationFunctionCollection);
			}
		}

		[TearDown]
		public void Teardown()
		{
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				new ApplicationFunctionRepository(uow).Remove(applicationFunction);
				new ApplicationRoleRepository(uow).Remove(applicationRole);
				uow.PersistAll();
			}
			dataSource.Dispose();
		}
	}
}