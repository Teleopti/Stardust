using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class SaveUserTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		//public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldUpdate()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			//using (TenantUnitOfWork.Start())
			//{
			//	var result = Target.Login(model).Content;
			//	result.Success.Should().Be.False();
			//	result.Message.Should().Be.EqualTo("No user with that Email.");
			//}
		}

	}
}