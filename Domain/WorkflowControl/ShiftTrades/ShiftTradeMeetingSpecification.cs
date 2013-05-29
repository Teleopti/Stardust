using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeMeetingSpecification : ShiftTradeSpecification
	{
		public override string DenyReason
		{
			get { return "ShiftTradeMeetingSpecificationDenyReason"; }
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if(obj == null)
				throw new ArgumentNullException("obj");

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
					var periodFrom = shiftFrom.LayerCollection.Period();
					if (!periodFrom.HasValue) continue;

					if (!checkCover(personMeetingsTo, periodFrom.Value))
						return false;
				}

				if (shiftTo != null)
				{
					var periodTo = shiftTo.LayerCollection.Period();
					if (!periodTo.HasValue) continue;

					if (!checkCover(personMeetingsFrom, periodTo.Value))
						return false;
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
