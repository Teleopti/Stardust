using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public class EarlyStartTimeCellValidator<T> : TimeLimitCellValidatorBase<T>
		where T : ScheduleRestrictionBaseView
	{
		public override bool ValidateCell(T dataItem)
		{
			StartTimeLimitation startTimeLimit = dataItem.StartTimeLimit();
			EndTimeLimitation endTimeLimit = dataItem.EndTimeLimit();

			bool validWithEnd = ScheduleRestrictionBaseView.IsValidRange(startTimeLimit.StartTime, startTimeLimit.EndTime);
			bool validWithLateStart = ScheduleRestrictionBaseView.IsValidRange(startTimeLimit.StartTime, endTimeLimit.EndTime);
			bool validRange = validWithEnd && validWithLateStart;

			Canceled = !validRange;

			if (Canceled)
			{
				if (!validWithEnd)
				{
					CreateErrorTip(startTimeLimit.StartTimeString, startTimeLimit.EndTimeString);
				}
				else if (!validWithLateStart)
				{
					CreateErrorTip(startTimeLimit.StartTimeString, endTimeLimit.EndTimeString);
				}
			}

			return validRange;
		}
	}
}