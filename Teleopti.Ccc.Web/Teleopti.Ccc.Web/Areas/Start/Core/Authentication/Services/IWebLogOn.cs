using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IWebLogOn
	{
		void LogOn(Guid businessUnitId, string dataSourceName, Guid personId, AuthenticationTypeOption authenticationType);
	}
}