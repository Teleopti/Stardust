using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    /// <summary>
    /// Represents a .
    /// </summary>
    public class LateStartTimeCellValidator<T> : TimeLimitCellValidatorBase<T>
        where T : ScheduleRestrictionBaseView
    {
        public override bool ValidateCell(T dataItem)
        {
            StartTimeLimitation startTimeLimit = dataItem.StartTimeLimit();
			
			bool validWithEarlyEnd = ScheduleRestrictionBaseView.IsValidRange(startTimeLimit.StartTime, startTimeLimit.EndTime);

			Canceled = !validWithEarlyEnd;
            if (Canceled)
            {
				CreateErrorTip(startTimeLimit.StartTimeString, startTimeLimit.EndTimeString);
            }

			return validWithEarlyEnd;
        }
    }
}