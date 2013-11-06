using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeDateSpecification : ShiftTradeSpecification
	{
		public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
		{
			foreach (var shiftTradeSwapDetail in obj)
			{				
				if (shiftTradeSwapDetail.SchedulePartFrom.PersonAssignment() != null &&
				    shiftTradeSwapDetail.SchedulePartTo.PersonAssignment() != null)
				{
					var fromStartUtc = shiftTradeSwapDetail.SchedulePartFrom.PersonAssignment().Period.StartDateTime;
					var toStartUtc = shiftTradeSwapDetail.SchedulePartTo.PersonAssignment().Period.StartDateTime;

					var sendersTimeZone = shiftTradeSwapDetail.PersonFrom.PermissionInformation.DefaultTimeZone();
					var recieversTimeZone = shiftTradeSwapDetail.PersonTo.PermissionInformation.DefaultTimeZone();

					var senderStartCurrent = TimeZoneInfo.ConvertTimeFromUtc(fromStartUtc, sendersTimeZone);
					var senderStartAfterTrade = TimeZoneInfo.ConvertTimeFromUtc(toStartUtc, sendersTimeZone);

					var receiverStartCurrent = TimeZoneInfo.ConvertTimeFromUtc(toStartUtc, recieversTimeZone);
					var receiverStartAfterTrade = TimeZoneInfo.ConvertTimeFromUtc(fromStartUtc, recieversTimeZone);

					if (senderStartCurrent.Date != senderStartAfterTrade.Date || receiverStartCurrent.Date != receiverStartAfterTrade.Date) return false;
				}
				
			}
			return true;
		}

		public override string DenyReason
		{
			get { return "ShiftTradeDifferentDateDenyReason"; }
		}

		
	}
}