using System;
using Teleopti.Ccc.WinCode.Settings;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public class EarlyEndTimeCellValidator<T> : TimeLimitCellValidatorBase<T>
        where T : ScheduleRestrictionBaseView
    {
        public override bool ValidateCell(T dataItem)
        {
            var endTimeLimit = dataItem.EndTimeLimit();

            var validWithLateStart = ScheduleRestrictionBaseView.IsValidRange(endTimeLimit.StartTime, endTimeLimit.EndTime);
			Canceled = !validWithLateStart;

            if (Canceled)
            {
                CreateErrorTip(endTimeLimit.StartTimeString, endTimeLimit.EndTimeString);
            }

			return validWithLateStart;
        }
    }
}