using System;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IWebLogOn
	{
		void LogOn(string dataSourceName, Guid businessUnitId, Guid personId, string tenantPassword, bool isPersistent);
	}
}