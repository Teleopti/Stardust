using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public interface IPermissionsConverter
	{
		ICollection<AnalyticsPermission> GetApplicationPermissionsAndConvert(Guid personId, int analyticsBusinessUnitId);
	}
}