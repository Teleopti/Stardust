﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class PersonInfoMapperTest
	{
		[Test]
		public void PersonIdShouldBeSet()
		{
			var id = Guid.NewGuid();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { PersonId = id });
			result.Id.Should().Be.EqualTo(id);
		}

		[Test]
		public void NullPersonIdShouldBeSetToDefaultValue()
		{
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { PersonId = null });
			result.Id.Should().Be.EqualTo(Guid.Empty);
		}

		[Test]
		//TODO: tenant - unique identity will be checked on db level currently - enough?
		public void IdentityShouldBeSet()
		{
			var identity = RandomName.Make();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { Identity = identity });
			result.Identity.Should().Be.EqualTo(identity);
		}

		[Test]
		//TODO: tenant - unique application logon will be checked on db level currently - enough?
		public void ApplicationLogonShouldBeSet()
		{
			var applicationLogon = RandomName.Make();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { UserName = applicationLogon });
			result.ApplicationLogonName.Should().Be.EqualTo(applicationLogon);
		}

		[Test]
		//TODO: tenant - what about password policies? currently on checked on client side.
		public void PasswordShouldBeSet()
		{
			var password = RandomName.Make();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { Password = password });
			result.Password.Should().Be.EqualTo(password);
		}

		[Test]
		public void TerminalDateShouldBeSet()
		{
			var terminalDate = DateOnly.Today;
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { TerminalDate = terminalDate});
			result.TerminalDate.Should().Be.EqualTo(terminalDate);
		}

		[Test]
		public void NullTerminalDateShouldBeSet()
		{
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { TerminalDate = null });
			result.TerminalDate.HasValue.Should().Be.False();
		}
	}
}