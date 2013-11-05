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
			foreach (var shiftTradeSwapDetail in obj)
			{				
				var fromStartUtc = shiftTradeSwapDetail.SchedulePartFrom.Period.StartDateTime;
				var toStartUtc = shiftTradeSwapDetail.SchedulePartTo.Period.StartDateTime;

				var sendersTimeZone = shiftTradeSwapDetail.PersonFrom.PermissionInformation.DefaultTimeZone();
				var recieversTimeZone = shiftTradeSwapDetail.PersonFrom.PermissionInformation.DefaultTimeZone();

				var myStartMyShift = TimeZoneInfo.ConvertTimeFromUtc(fromStartUtc, sendersTimeZone).Date;
				var myStartOtherShift = TimeZoneInfo.ConvertTimeFromUtc(toStartUtc, sendersTimeZone).Date;

				var otherStartHisShift = TimeZoneInfo.ConvertTimeFromUtc(toStartUtc, recieversTimeZone).Date;
				var otherStartMysShift = TimeZoneInfo.ConvertTimeFromUtc(fromStartUtc, recieversTimeZone).Date;

				if (myStartMyShift != myStartOtherShift || otherStartHisShift != otherStartMysShift) return false;



			}
			return true;
		}

		public override string DenyReason
		{
			get { return "TODO"; }
		}

		
	}
}
