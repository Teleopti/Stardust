using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Wfm.Azure.Common;
using NotificationMessage = Teleopti.Ccc.Web.Core.Startup.NotificationMessage;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[DomainTest]
	[Toggle(Toggles.Wfm_AutomaticNotificationEnrollment_79679)]
	public class NotificationSubscriptionTaskTest
	{
		public FakeTenants LoadAllTenants;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public FakeNotificationServiceClient NotificationServiceClient;
		public ApplicationConfigurationDbProviderFake ApplicationConfigurationDbProvider;
		public FakeInstallationEnvironment InstallationEnvironment;

		[Test]
		public void ShouldNotRunOnLaterInstanceThanOne()
		{
			ApplicationConfigurationDbProvider
				.LoadFakeData(new ApplicationConfigurationDb
				{
					Server = new Dictionary<ServerConfigurationKey, string>
					{
						{ ServerConfigurationKey.NotificationSubscriptionApiEndpointKey , "asdfghjkl"},
						{ ServerConfigurationKey.NotificationSubscriptionApiEndpoint , "http://noway.se"},
					},
					Tenant = new Dictionary<TenantApplicationConfigKey, string>
					{
						{ TenantApplicationConfigKey.NotificationApiKey, "" }
					}
				});

			LoadAllTenants.Has("FakeTenant1");
			LoadAllTenants.Has("FakeTenant2");
			LoadAllTenants.Has("FakeTenant3");

			var env = new FakeInstallationEnvironment
			{
				IsAzure = true,
				RoleInstanceID = 7
			};

			var task = new NotificationSubscriptionTask(LoadAllTenants, TenantUnitOfWork, NotificationServiceClient, ApplicationConfigurationDbProvider, env);
			task.Execute(null).GetAwaiter().GetResult();

			Assert.IsTrue(task.LastExecution.Message == NotificationMessage.NotOnFirstRoleInstanceID);
		}

		[Test]
		public void ShouldHaveASubscriptionKeyIfCloud()
		{
			ApplicationConfigurationDbProvider
				.LoadFakeData(new ApplicationConfigurationDb
				{
					Server = new Dictionary<ServerConfigurationKey, string>
					{
						{ ServerConfigurationKey.NotificationSubscriptionApiEndpointKey , "asdfghjkl"},
						{ ServerConfigurationKey.NotificationSubscriptionApiEndpoint , "http://noway.se"},
					},
					Tenant = new Dictionary<TenantApplicationConfigKey, string>
					{
						{ TenantApplicationConfigKey.NotificationApiKey, "" }
					}
				});

			LoadAllTenants.Has("FakeTenant1");
			LoadAllTenants.Has("FakeTenant2");
			LoadAllTenants.Has("FakeTenant3");
			
			var env = new FakeInstallationEnvironment
			{
				IsAzure = true,
				RoleInstanceID = 0
			};

			var task = new NotificationSubscriptionTask(LoadAllTenants, TenantUnitOfWork, NotificationServiceClient, ApplicationConfigurationDbProvider, env);
			task.Execute(null).GetAwaiter().GetResult();

			Assert.IsTrue(task.LastExecution.Keys.Any());
		}

		[Test]
		public void ShouldNotRunIfSubscriptionApiIsNotPresent()
		{
			var env = new FakeInstallationEnvironment
			{
				IsAzure = true,
				RoleInstanceID = 0
			};

			var db = new ApplicationConfigurationDbProviderFake();
			db.LoadFakeData(new ApplicationConfigurationDb
			{
				Server = new Dictionary<ServerConfigurationKey, string>
				{
				},
				Tenant = new Dictionary<TenantApplicationConfigKey, string>
				{
					{ TenantApplicationConfigKey.NotificationApiKey, "" }
				}
			});

			var task = new NotificationSubscriptionTask(LoadAllTenants, TenantUnitOfWork, NotificationServiceClient, db, env);
			task.Execute(null).GetAwaiter().GetResult();

			Assert.IsTrue(!task.LastExecution.IsSuccess && task.LastExecution.Message == NotificationMessage.ServerConfigurationKeysNotPresent);
		}

		[Test]
		public void ShouldNotFetchNewKeyIfExist()
		{

			var loadFakeTenantsLocal = new FakeTenants();
			var fakeTenant = new Tenant("TenantName");
			fakeTenant.SetApplicationConfig(TenantApplicationConfigKey.NotificationApiKey, "Test");

			loadFakeTenantsLocal.Has(fakeTenant);

			var env = new FakeInstallationEnvironment
			{
				IsAzure = true,
				RoleInstanceID = 0
			};

			var db = new ApplicationConfigurationDbProviderFake();
			db.LoadFakeData(new ApplicationConfigurationDb
			{
				Server = new Dictionary<ServerConfigurationKey, string>
				{
					{ ServerConfigurationKey.NotificationSubscriptionApiEndpointKey , "asdfghjkl"},
					{ ServerConfigurationKey.NotificationSubscriptionApiEndpoint , "http://noway.se"},
				},
			});

			var task = new NotificationSubscriptionTask(loadFakeTenantsLocal, TenantUnitOfWork, NotificationServiceClient, db, env);
			task.Execute(null).GetAwaiter().GetResult();

			Assert.AreEqual(0, task.LastExecution.Keys.Count);


		}
	}
}
