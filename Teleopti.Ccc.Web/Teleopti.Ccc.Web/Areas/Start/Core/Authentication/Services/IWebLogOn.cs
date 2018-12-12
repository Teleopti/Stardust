using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IWebLogOn
	{
		void LogOn(string dataSourceName, Guid businessUnitId, IPerson person, string tenantPassword, bool isPersistent, bool isLogonFromBrowser);
	}
}