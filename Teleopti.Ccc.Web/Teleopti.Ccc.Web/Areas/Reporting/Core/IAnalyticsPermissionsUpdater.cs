using System;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public interface IAnalyticsPermissionsUpdater
	{
		void Handle(Guid personId, Guid businessUnitId);
	}
}