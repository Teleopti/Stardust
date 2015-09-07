using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GetImportUsersTest
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants LoadAllTenants;

		private const string dummy = "CF0DA4E0-DC93-410B-976B-EED9C8A34639";

		[Test]
		public void ShouldReadUsersFromDatabaseToImport()
		{
			//create database
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");

			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
				{
					InitialCatalog = dummy
				};

			string connString = connStringBuilder.ConnectionString;
			var helper = new DatabaseHelper(connString, DatabaseType.TeleoptiCCC7);
			helper.CreateLogin("appuser", "SomeG00dpw", false);
			if (!helper.Exists())
			{
				helper.CreateByDbManager();
				helper.CreateSchemaByDbManager();
				helper.AddPermissions("appuser", false);
			}

			// another
			var tenantUowManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(connString);
			tenantUowManager.Start();
			
			var loadAllTenants = new LoadAllTenants(tenantUowManager);
			var tenant = loadAllTenants.Tenants().FirstOrDefault();
			if (tenant == null)
			{
				tenant = new Tenant("Importing");
				tenantUowManager.CurrentSession().Save(tenant);
			}
			var randomName = RandomName.Make();

			//add some personinfos (if nonone)
			var loadAllPersonInfos = new LoadAllPersonInfos(tenantUowManager);
			var infos = loadAllPersonInfos.PersonInfos().ToList();
			if (!infos.Any())
			{
				var pInfo = new PersonInfo(tenant, Guid.NewGuid());
				pInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), randomName, RandomName.Make());
				tenantUowManager.CurrentSession().Save(pInfo);
				
			}
			else
			{
				randomName = infos.First().ApplicationLogonInfo.LogonName;
			}
			tenantUowManager.CommitAndDisposeCurrent();

			TenantUnitOfWork.Start();
			var mainTenant = LoadAllTenants.Tenants().First();
			var personInfo = new PersonInfo(mainTenant, Guid.NewGuid());
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), randomName, RandomName.Make());
			CurrentTenantSession.CurrentSession().Save(personInfo);
			TenantUnitOfWork.CommitAndDisposeCurrent();
			var conflicting =
				Target.Conflicts(new ImportDatabaseModel
				{
					Server = connStringBuilder.DataSource,
					AppDatabase = connStringBuilder.InitialCatalog,
					UserName = "appuser",
					Password = "SomeG00dpw",
					Tenant = "Importing"
				});
			conflicting.Content.ConflictingUserModels.Count().Should().Be.GreaterThan(0);

		}
	}
}