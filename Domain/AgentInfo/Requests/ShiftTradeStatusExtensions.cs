using System;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public static class ShiftTradeStatusExtensions
	{
		public static string ToText(this ShiftTradeStatus status, bool isCreatedByUser)
		{
			switch (status)
			{
				case ShiftTradeStatus.OkByBothParts:
					{
						return Resources.WaitingForSupervisorApproval;
					}
				case ShiftTradeStatus.OkByMe:
					{
						if(isCreatedByUser) return Resources.WaitingForOtherPart;
						return Resources.WaitingForYourApproval;
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