using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GenerateRtaKeyForTenant : ISetup
	{
		public DatabaseController Database;
		public ImportController Import;
		public ILoadAllTenants Tenants;
		public ITenantUnitOfWork TenantUnitOfWork;
		public TestPollutionCleaner TestPollutionCleaner;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DatabaseHelperWrapperFake>().For<IDatabaseHelperWrapper>();
			system.UseTestDouble<CheckDatabaseVersionsFake>().For<ICheckDatabaseVersions>();
			system.UseTestDouble<UpdateCrossDatabaseViewFake>().For<IUpdateCrossDatabaseView>();
			system.UseTestDouble<GetImportUsersFake>().For<IGetImportUsers>();
		}

		[Test]
		public void ShouldGenerateRtaKeyWhenCreatingDatabases()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			TestPollutionCleaner.Clean("tenant", "appuser");

			var result = Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant",
			});

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
				Tenants.Tenants().Single(x => x.Name == "tenant").RtaKey.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldGenerateRtaKeyWith10RandomCharacters()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			TestPollutionCleaner.Clean("tenant1", "appuser1");
			TestPollutionCleaner.Clean("tenant2", "appuser2");

			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant1",
				AppUser = "appuser1",
			});
			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant2",
				AppUser = "appuser2",
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