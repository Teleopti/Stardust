using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ActivityAndIntervalLength
	{
		public ActivityAndIntervalLength(IActivity activity, TimeSpan intervalLength)
		{
			Activity = activity;
			IntervalLength = intervalLength;
		}

		public IActivity Activity { get; }
		public TimeSpan IntervalLength { get; }
	}
}