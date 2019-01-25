using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    public abstract class TimeLimitCellValidatorBase<T> : SFGridCellValidatorBase<T>
        where T : ScheduleRestrictionBaseView
    {
        protected override CultureInfo CurrentCulture
        {
            get
            {
                return TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture;
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