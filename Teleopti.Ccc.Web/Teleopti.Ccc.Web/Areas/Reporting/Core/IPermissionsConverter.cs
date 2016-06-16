using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public interface IPermissionsConverter
	{
		IEnumerable<AnalyticsPermission> GetApplicationPermissionsAndConvert(Guid personId, int analyticsBusinessUnitId);
	}
}