using System;

namespace Teleopti.Ccc.Domain.Security
{
	public static class SystemUser
	{
		public static readonly Guid Id = new Guid("3f0886ab-7b25-4e95-856a-0d726edc2a67");
		public static readonly string Name = "System";

		public static readonly Guid SuperRoleId = new Guid("193AD35C-7735-44D7-AC0C-B8EDA0011E5F");
		public static readonly string SuperRoleName = "_Super Role";
	}
}
