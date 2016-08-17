﻿using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAllLicenseActivatorProvider : ILicenseActivatorProvider
	{
		public ILicenseActivator Current()
		{
			return new FakeAllLicenseActivator();
		}
	}
}