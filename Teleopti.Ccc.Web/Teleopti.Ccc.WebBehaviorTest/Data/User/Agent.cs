﻿using System.Globalization;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class Agent : IUserSetup, IUserRoleSetup
	{
		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.AddApplicationRole(TestData.AgentRole);
		}
	}
}