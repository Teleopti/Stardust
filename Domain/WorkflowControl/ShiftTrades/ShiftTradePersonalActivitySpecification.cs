using System;
using System.Collections.Generic;
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
				var personAssignmentFrom = shiftTradeSwapDetail.SchedulePartFrom.AssignmentHighZOrder();
				if (personAssignmentFrom == null) continue;
				var mainShiftFrom = personAssignmentFrom.ToMainShift();
				
				var personAssignmentTo = shiftTradeSwapDetail.SchedulePartTo.AssignmentHighZOrder();
				if (personAssignmentTo == null) continue;
				var mainShiftTo = personAssignmentTo.ToMainShift();

				if (mainShiftFrom != null)
				{
					var periodFrom = mainShiftFrom.LayerCollection.Period();
					if (!periodFrom.HasValue) continue;

					if (!CheckCover(personAssignmentTo, periodFrom.Value))
						return false;
				}

				if (mainShiftTo != null)
				{
					var periodTo = mainShiftTo.LayerCollection.Period();
					if (!periodTo.HasValue) continue;

					if (!CheckCover(personAssignmentFrom, periodTo.Value))
						return false;
				}	
			}

			return true;
		}

		private static bool CheckCover(IPersonAssignment personAssignment, DateTimePeriod period)
		{
			foreach (var personalShift in personAssignment.PersonalShiftCollection)
			{
				if (!personalShift.HasProjection) continue;

				var personalShiftPeriod = personalShift.LayerCollection.Period();
				if (!personalShiftPeriod.HasValue) continue;

				if (!period.Contains(personalShiftPeriod.Value))
					return false;
			}

			return true;
		}
	}
}
