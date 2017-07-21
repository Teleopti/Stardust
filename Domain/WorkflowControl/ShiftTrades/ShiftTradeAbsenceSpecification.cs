using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeAbsenceSpecification : ShiftTradeSpecification
	{
		public override string DenyReason => "ShiftTradeAbsenceDenyReason";

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			foreach (var shiftTradeSwapDetail in obj)
			{
				var visualLayerCollectionFrom = shiftTradeSwapDetail.SchedulePartFrom.ProjectionService().CreateProjection();

				if (visualLayerCollectionFrom.Select(visualLayer => visualLayer.Payload).OfType<Absence>().Any())
					return false;

				var visualLayerCollectionTo = shiftTradeSwapDetail.SchedulePartTo.ProjectionService().CreateProjection();

				if (visualLayerCollectionTo.Select(visualLayer => visualLayer.Payload).OfType<Absence>().Any())
					return false;
			}

			return true;
		}
	}
}
