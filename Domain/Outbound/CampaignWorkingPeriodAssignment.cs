using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class CampaignWorkingPeriodAssignment : AggregateEntity
	{
		private DayOfWeek _weekdayIndex;

		public virtual DayOfWeek WeekdayIndex
		{
			get { return _weekdayIndex; }
			set { _weekdayIndex = value; }
		}
	}
}
