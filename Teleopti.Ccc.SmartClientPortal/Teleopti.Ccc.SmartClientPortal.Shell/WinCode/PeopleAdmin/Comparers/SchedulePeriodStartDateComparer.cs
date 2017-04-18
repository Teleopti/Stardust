using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	public class SchedulePeriodStartDateComparer : IComparer<SchedulePeriodModel>
	{
		public int Compare(SchedulePeriodModel x, SchedulePeriodModel y)
		{
			int result = 0;

			if (x.PeriodDate.HasValue && y.PeriodDate.HasValue)
			{
				result = DateTime.Compare(x.PeriodDate.Value.Date, y.PeriodDate.Value.Date);
			}
			else if (!x.PeriodDate.HasValue && y.PeriodDate.HasValue)
			{
				result = -1;
			}
			else if (!y.PeriodDate.HasValue && x.PeriodDate.HasValue)
			{
				result = 1;
			}

			return result;
		}
	}
}
