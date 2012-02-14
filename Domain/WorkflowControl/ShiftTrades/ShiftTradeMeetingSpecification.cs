using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeMeetingSpecification : ShiftTradeSpecification, IShiftTradeMeetingSpecification
	{
		public override string DenyReason
		{
			get { return "ShiftTradeMeetingSpecificationDenyReason"; }
		}

		public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
		{
			if(obj == null)
				throw new ArgumentNullException("obj");

			foreach (var shiftTradeSwapDetail in obj)
			{
				var personAssignmentFrom = shiftTradeSwapDetail.SchedulePartFrom.AssignmentHighZOrder();
				var personMeetingsFrom = shiftTradeSwapDetail.SchedulePartFrom.PersonMeetingCollection();
				if (personAssignmentFrom == null && personMeetingsFrom.Count == 0) continue;

				var personAssignmentTo = shiftTradeSwapDetail.SchedulePartTo.AssignmentHighZOrder();
				var personMeetingsTo = shiftTradeSwapDetail.SchedulePartTo.PersonMeetingCollection();
				if(personAssignmentTo == null && personMeetingsTo.Count == 0) continue;

				if(personAssignmentFrom != null && personAssignmentFrom.MainShift != null)
				{
					var periodFrom = personAssignmentFrom.MainShift.LayerCollection.Period();
					if (!periodFrom.HasValue) continue;

					if (!CheckCover(personMeetingsTo, periodFrom.Value))
						return false;
				}

				if(personAssignmentTo != null && personAssignmentTo.MainShift != null)
				{
					var periodTo = personAssignmentTo.MainShift.LayerCollection.Period();
					if (!periodTo.HasValue) continue;

					if (!CheckCover(personMeetingsFrom, periodTo.Value))
						return false;
				}
			}

			return true;
		}

		private static bool CheckCover(IEnumerable<IPersonMeeting> personMeetings, DateTimePeriod period)
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
