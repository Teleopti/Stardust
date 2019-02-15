using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class GenerateRtaKeyForTenant : IIsolateSystem
	{
		public DatabaseController Database;
		public ImportController Import;
		public ILoadAllTenants Tenants;
		public ITenantUnitOfWork TenantUnitOfWork;
		public TestPollutionCleaner TestPollutionCleaner;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<DatabaseHelperWrapperFake>().For<IDatabaseHelperWrapper>();
			isolate.UseTestDouble<CheckDatabaseVersionsFake>().For<ICheckDatabaseVersions>();
			isolate.UseTestDouble<UpdateCrossDatabaseViewFake>().For<IUpdateCrossDatabaseView>();
			isolate.UseTestDouble<GetImportUsersFake>().For<IGetImportUsers>();
			isolate.UseTestDouble<CreateBusinessUnitFake>().For<ICreateBusinessUnit>();
		}
		
		[Test]
		public void ShouldGenerateRtaKeyWhenCreatingDatabases()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			TestPollutionCleaner.Clean("tenant", "appuser");

			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant",
				AppPassword = "Passw0rd"
			});

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				Tenants.Tenants().Single(x => x.Name == "tenant").RtaKey.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldGenerateRtaKeyWith10RandomCharacters()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			TestPollutionCleaner.Clean("tenant1", "appuser1");
			TestPollutionCleaner.Clean("tenant2", "appuser2");

			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant1",
				AppUser = "appuser1",
				AppPassword = "Passw0rd"
			});
			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant2",
				AppUser = "appuser2",
				AppPassword = "Passw0rd",
				FirstUser = "seconduser"
			});

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var key1 = Tenants.Tenants().Single(x => x.Name == "tenant1").RtaKey;
				var key2 = Tenants.Tenants().Single(x => x.Name == "tenant2").RtaKey;
				key1.Should().Match(new Regex("[A-Za-z0-9]{10}"));
				key2.Should().Match(new Regex("[A-Za-z0-9]{10}"));
				key1.Should().Not.Be.EqualTo(key2);
			}
		}

	}
}