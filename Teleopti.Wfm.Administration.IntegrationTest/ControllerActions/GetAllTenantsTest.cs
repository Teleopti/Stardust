using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[TenantTest]
	public class GetAllTenantsTest
	{
		public HomeController Target;

		[Test]
		public void ShouldNotBeNull()
		{
			//create database
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());

			Target.GetAllTenants().Should().Not.Be.Null();
		}
	}
}