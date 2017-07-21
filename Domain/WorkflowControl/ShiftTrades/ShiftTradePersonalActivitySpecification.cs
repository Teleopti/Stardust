using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradePersonalActivitySpecification : ShiftTradeSpecification
	{
		public override string DenyReason => "ShiftTradePersonalActivityDenyReason";

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if(obj == null)
				throw new ArgumentNullException(nameof(obj));

			foreach (var shiftTradeSwapDetail in obj)
			{
				var personAssignmentFrom = shiftTradeSwapDetail.SchedulePartFrom.PersonAssignment();
				if (personAssignmentFrom == null)
					continue;
				var mainShiftFrom = shiftTradeSwapDetail.SchedulePartFrom.GetEditorShift();
				
				var personAssignmentTo = shiftTradeSwapDetail.SchedulePartTo.PersonAssignment();
				if (personAssignmentTo == null) continue;
				var mainShiftTo = shiftTradeSwapDetail.SchedulePartTo.GetEditorShift();

				if (mainShiftFrom != null)
				{
					if (mainShiftFrom.LayerCollection.PeriodBlocks()
								.Any(periodFrom => !checkCover(personAssignmentTo, periodFrom)))
					{
						return false;
					}
				}

				if (mainShiftTo != null)
				{
					if (mainShiftTo.LayerCollection.PeriodBlocks()
								.Any(periodTo => !checkCover(personAssignmentFrom, periodTo)))
					{
						return false;
					}
				}	
			}

			return true;
		}

		private static bool checkCover(IPersonAssignment personAssignment, DateTimePeriod period)
		{
			return personAssignment.PersonalActivities().All(personalLayer => period.Contains(personalLayer.Period));
		}
	}
}
