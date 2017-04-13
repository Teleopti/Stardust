using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public class LateEndTimeCellValidator<T> : TimeLimitCellValidatorBase<T>
        where T : ScheduleRestrictionBaseView
    {
        public override bool ValidateCell(T dataItem)
        {
            StartTimeLimitation startTimeLimit = dataItem.StartTimeLimit();
            EndTimeLimitation endTimeLimit = dataItem.EndTimeLimit();

            bool validWithEnd = ScheduleRestrictionBaseView.IsValidRange(endTimeLimit.StartTime, endTimeLimit.EndTime);
            bool validWithEarlyStart = ScheduleRestrictionBaseView.IsValidRange(startTimeLimit.StartTime, endTimeLimit.EndTime);
            bool validRange = validWithEnd && validWithEarlyStart;

            Canceled = !validRange;

            if (Canceled)
            {
                if (!validWithEnd)
                {
                    CreateErrorTip(endTimeLimit.StartTimeString, endTimeLimit.EndTimeString);
                }
                else if (!validWithEarlyStart)
                {
                    CreateErrorTip(startTimeLimit.StartTimeString, endTimeLimit.EndTimeString);
                }
            }

            return validRange;
        }
    }
}