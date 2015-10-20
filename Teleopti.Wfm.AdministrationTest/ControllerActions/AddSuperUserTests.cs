﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class AddSuperUserTests
	{
		public DatabaseController Target;

		[Test]
		public void ShouldReturnFalseWhenNoTenant()
		{
			var result = Target.AddSuperUserToTenant(new AddSuperUserToTenantModel());
			result.Content.Success.Should().Be.False();
			//if this happen we do it wrong in the javascript
			result.Content.Message.Should().Be.EqualTo("The Tenant name can not be empty.");
		}

		[Test]
		public void ShouldReturnFalseWhenNoTenantInDb()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			var result = Target.AddSuperUserToTenant(new AddSuperUserToTenantModel {Tenant = "tenantThatNotExists", UserName = "userName", Password = "PassaDej0"});
			result.Content.Success.Should().Be.False();
			//if this happen we do it wrong in the javascript
			result.Content.Message.Should().Be.EqualTo("Can not find this Tenant in the database.");
		}

		[Test]
		public void ShouldReturnFalseWhenNoUsernameOrPassword()
		{
			var result = Target.AddSuperUserToTenant(new AddSuperUserToTenantModel { Tenant = "TestData" });
			result.Content.Success.Should().Be.False();
			result.Content.Message.Should().Be.EqualTo("The user name can not be empty.");
		}

		[Test]
		public void ShouldReturnTrueWhenUsernameAndPasswordProvided()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			var result =
				Target.AddSuperUserToTenant(new AddSuperUserToTenantModel
				{
					Tenant = "TestData",
					FirstName = "Adam",
					LastName = "Adamsson",
					UserName = "Kilroy",
					Password = "WasHere"
				});
			result.Content.Message.Should().Be.EqualTo("Created new user.");
			result.Content.Success.Should().Be.True();
			
		}
	}
}