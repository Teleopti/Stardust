using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class GetAllTenantsTest
	{
		public HomeController Target;

		[Test]
		public void ShouldNotBeNull()
		{
			//create database
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());

			Target.GetAllTenants().Should().Not.Be.Null();
		}
	}
}