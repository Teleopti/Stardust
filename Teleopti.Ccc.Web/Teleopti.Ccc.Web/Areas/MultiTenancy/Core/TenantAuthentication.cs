﻿using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class TenantAuthentication : ITenantAuthentication
	{
		public bool HasAccess()
		{
			return true;
		}
	}
}