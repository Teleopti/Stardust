using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Settings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
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