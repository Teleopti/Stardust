using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradePersonalActivitySpecification : ShiftTradeSpecification
	{
		public override string DenyReason
		{
			get { return "ShiftTradePersonalActivityDenyReason"; }
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if(obj == null)
				throw new ArgumentNullException("obj");

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
					var periodFrom = mainShiftFrom.LayerCollection.Period();
					if (!periodFrom.HasValue) continue;

					if (!checkCover(personAssignmentTo, periodFrom.Value))
						return false;
				}

				if (mainShiftTo != null)
				{
					var periodTo = mainShiftTo.LayerCollection.Period();
					if (!periodTo.HasValue) continue;

					if (!checkCover(personAssignmentFrom, periodTo.Value))
						return false;
				}	
			}

			return true;
		}

		private static bool checkCover(IPersonAssignment personAssignment, DateTimePeriod period)
		{
			return personAssignment.PersonalLayers().All(personalLayer => period.Contains(personalLayer.Period));
		}
	}
}
