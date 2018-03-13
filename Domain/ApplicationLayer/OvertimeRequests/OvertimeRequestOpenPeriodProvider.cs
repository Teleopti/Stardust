using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestOpenPeriodProvider : IOvertimeRequestOpenPeriodProvider
	{
		private  readonly INow _now;

		public OvertimeRequestOpenPeriodProvider(INow now)
		{
			_now = now;
		}

		public IList<IOvertimeRequestOpenPeriod> GetOvertimeRequestOpenPeriods(IPerson person, DateTimePeriod period)
		{
			return new List<IOvertimeRequestOpenPeriod>
			{
				getMergedOvertimeRequestOpenPeriod(person, period)
			};
		}

		private IOvertimeRequestOpenPeriod getMergedOvertimeRequestOpenPeriod(IPerson person, DateTimePeriod period)
		{
			var viewpointDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(),
				person.PermissionInformation.DefaultTimeZone()));
			return person.WorkflowControlSet?.GetMergedOvertimeRequestOpenPeriod(period, viewpointDate, person.PermissionInformation);
		}
	}
}