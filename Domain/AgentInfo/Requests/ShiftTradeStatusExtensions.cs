using System;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public static class ShiftTradeStatusExtensions
	{
		public static string ToText(this ShiftTradeStatus status)
		{
			switch (status)
			{
				case ShiftTradeStatus.OkByBothParts:
					{
						return Resources.WaitingForSupervisorApproval;
					}
				case ShiftTradeStatus.OkByMe:
					{
						return Resources.WaitingForOtherPart;
					}
				case ShiftTradeStatus.Referred:
					{
						return Resources.TheScheduleHasChanged;
					}
				default:
					{
						//ShiftTradeStatus.NotValid verkar inte användas
						throw new ArgumentException("Unknown shift trade status " + status);
					}
			}
		}
	}
}