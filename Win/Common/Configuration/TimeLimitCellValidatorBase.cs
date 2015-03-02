using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using System.Globalization;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public abstract class TimeLimitCellValidatorBase<T> : SFGridCellValidatorBase<T>
        where T : ScheduleRestrictionBaseView
    {
        protected override CultureInfo CurrentCulture
        {
            get
            {
                return TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
            }
        }

        protected void CreateErrorTip(string fromTime, string toTime)
        {
            Message = string.Format(
                CurrentCulture,
                Resources.ErrorMessageFromToTimeIsInvalidParameter,
                fromTime,
                toTime
                );
        }
    }
}