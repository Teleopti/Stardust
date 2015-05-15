using System;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class EnvironmentWindowsUserProvider : IWindowsUserProvider
	{
		public string Identity()
		{
			return Environment.UserDomainName + "\\" + Environment.UserName;
		}
	}
}