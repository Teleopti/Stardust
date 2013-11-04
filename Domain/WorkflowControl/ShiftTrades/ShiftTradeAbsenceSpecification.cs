using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeAbsenceSpecification : ShiftTradeSpecification
	{
		public override string DenyReason
		{
			get { return "ShiftTradeAbsenceDenyReason"; }
		}

		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

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

	public class ShiftTradeMustBeSameDateInLocalTime : ShiftTradeSpecification
	{
		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			return obj.All(shiftTradeSwapDetail => shiftTradeSwapDetail.SchedulePartFrom.DateOnlyAsPeriod.DateOnly == shiftTradeSwapDetail.SchedulePartTo.DateOnlyAsPeriod.DateOnly);
		}

		public override string DenyReason
		{
			get { throw new NotImplementedException(); }
		}
	}
}
