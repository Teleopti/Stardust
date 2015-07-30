using System;
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
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldUpdate()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			int id;
			using (TenantUnitOfWork.Start())
			{
				var user = new TenantAdminUser {AccessToken = "dummy",Email = "ola@teleopti.com", Name = "Ola", Password = "vadsomhelst"};
				CurrentTenantSession.CurrentSession().Save(user);
				id = user.Id;
			}
			using (TenantUnitOfWork.Start())
			{
				var loaded = Target.GetOneUser(id);
				loaded.Content.Email.Should().Be.EqualTo("ola@teleopti.com");
				Target.SaveUser(new UpdateUserModel {Email = "olle@teleopti.com", Id = id, Name = "Olle"});
			}

			using (TenantUnitOfWork.Start())
			{
				var loaded = Target.GetOneUser(id);
				loaded.Content.Email.Should().Be.EqualTo("olle@teleopti.com");
			}
		}

	}
}