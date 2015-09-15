using System;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IIdAuthentication
	{
		TenantAuthenticationResult Logon(Guid id);
	}
}