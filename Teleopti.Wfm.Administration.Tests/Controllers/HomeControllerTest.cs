using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.Tests.Controllers
{
	[TenantTest]
	public class HomeControllerTest
	{
		public HomeController Target;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void GetAllTenantsShouldNoBeNull()
		{
			//create database
			var dataSource = DataSourceHelper.CreateDataSource(new IMessageSender[] { }, "TestData");
			TenantUnitOfWork.Start();
			Target.GetAllTenants().Should().Not.Be.Null();
			TenantUnitOfWork.CommitAndDisposeCurrent();
		}

		
	}
}
