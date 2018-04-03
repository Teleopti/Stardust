using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestOpenPeriodProviderToggle47290Off : IOvertimeRequestOpenPeriodProvider
	{
		private  readonly INow _now;

		public OvertimeRequestOpenPeriodProviderToggle47290Off(INow now)
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