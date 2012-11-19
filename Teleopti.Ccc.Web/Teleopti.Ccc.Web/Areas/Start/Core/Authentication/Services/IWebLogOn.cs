using System;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IWebLogOn
	{
		void LogOn(Guid businessUnitId, string dataSourceName, Guid personId, string warningMessage = null);
		void LogOn(string dataSourceName, Guid businessUnitId, Guid personId);
	}
}