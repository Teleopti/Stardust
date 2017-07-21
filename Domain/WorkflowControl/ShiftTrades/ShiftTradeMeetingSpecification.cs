using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeMeetingSpecification : ShiftTradeSpecification
	{
		public override string DenyReason => "ShiftTradeMeetingSpecificationDenyReason";

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if(obj == null)
				throw new ArgumentNullException(nameof(obj));

			foreach (var shiftTradeSwapDetail in obj)
			{
				var partFrom = shiftTradeSwapDetail.SchedulePartFrom;
				var partTo = shiftTradeSwapDetail.SchedulePartTo;

				var personMeetingsFrom = partFrom.PersonMeetingCollection();
				var shiftFrom = partFrom.GetEditorShift();
				if (shiftFrom == null && personMeetingsFrom.Count == 0)
					continue;

				var personMeetingsTo = shiftTradeSwapDetail.SchedulePartTo.PersonMeetingCollection();
				var shiftTo = partTo.GetEditorShift();
				if (shiftTo == null && personMeetingsTo.Count == 0)
					continue;

				if (shiftFrom != null)
				{
					if (shiftFrom.LayerCollection.PeriodBlocks()
						.Any(periodFrom => !checkCover(personMeetingsTo, periodFrom)))
					{
						return false;
					}
				}

				if (shiftTo != null)
				{
					if (shiftTo.LayerCollection.PeriodBlocks()
						.Any(periodTo => !checkCover(personMeetingsFrom, periodTo)))
					{
						return false;
					}
				}
			}

			return true;
		}

		private static bool checkCover(IEnumerable<IPersonMeeting> personMeetings, DateTimePeriod period)
		{
			foreach (var personMeeting in personMeetings)
			{
				if(!period.Contains(personMeeting.Period))
					return false;
			}

			return true;
		}
	}
}
