using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeEmptyScheduleSpecification : ShiftTradeSpecification
	{
		public override string DenyReason
		{
			get { return "ShiftTradeEmptyScheduleDenyReason"; }
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			foreach (var shiftTradeSwapDetail in obj)
			{
				var personAssignmentFrom = shiftTradeSwapDetail.SchedulePartFrom.PersonAssignment();
				if (personAssignmentFrom == null)
					continue;
				var hasMainShiftFrom = shiftTradeSwapDetail.SchedulePartFrom.GetEditorShift() != null;
				var hasDayOffFrom = personAssignmentFrom.DayOff() != null;
				var visualLayerCollectionFrom = shiftTradeSwapDetail.SchedulePartFrom.ProjectionService().CreateProjection();
				var hasAbsenceFrom = visualLayerCollectionFrom.Select(visualLayer => visualLayer.Payload).OfType<Absence>().Any();
				var isScheduleNotEmptyFrom = hasDayOffFrom || hasMainShiftFrom || hasAbsenceFrom;

				var personAssignmentTo = shiftTradeSwapDetail.SchedulePartTo.PersonAssignment();
				if (personAssignmentTo == null) continue;
				var hasMainShiftTo = shiftTradeSwapDetail.SchedulePartTo.GetEditorShift() != null;
				var hasDayOffTo = personAssignmentTo.DayOff() != null;
				var visualLayerCollectionTo = shiftTradeSwapDetail.SchedulePartTo.ProjectionService().CreateProjection();
				var hasAbsenceTo = visualLayerCollectionTo.Select(visualLayer => visualLayer.Payload).OfType<Absence>().Any();
				var isScheduleNotEmptyTo = hasMainShiftTo || hasDayOffTo || hasAbsenceTo;

				if (!isScheduleNotEmptyFrom || !isScheduleNotEmptyTo)
					return false;
			}

			return true;
		}
	}
}
