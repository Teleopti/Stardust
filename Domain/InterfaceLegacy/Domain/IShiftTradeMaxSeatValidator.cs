

using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IShiftTradeMaxSeatValidator
	{
		bool Validate (ISite site, IScheduleDay scheduleDayIncoming,
			IScheduleDay scheduleDayOutgoing, List<IVisualLayer> incomingActivitiesRequiringSeat,
			IList<ISeatUsageForInterval> seatUsageOnEachIntervalDic, TimeZoneInfo timeZoneInfo);
	}
}
