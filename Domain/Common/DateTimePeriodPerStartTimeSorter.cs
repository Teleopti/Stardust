using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DateTimePeriodPerStartTimeSorter : IComparer<DateTimePeriod>
	{
		public int Compare(DateTimePeriod x, DateTimePeriod y)
		{
			return x.StartDateTime.CompareTo(y.StartDateTime);
		}
	}
}