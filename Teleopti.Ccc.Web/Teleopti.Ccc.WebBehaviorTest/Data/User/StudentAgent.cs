﻿using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class StudentAgent : IUserSetup
	{
		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.AddApplicationRole(TestData.AgentRole);
		}
	}
}