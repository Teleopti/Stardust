using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using System.Globalization;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    /// <summary>
    /// Represents a .
    /// </summary>
    public abstract class TimeLimitCellValidatorBase<T> : SFGridCellValidatorBase<T>
        where T : ScheduleRestrictionBaseView
    {
        /// <summary>
        /// Gets information about current culture.
        /// </summary>
        protected override CultureInfo CurrentCulture
        {
            get
            {
                return TeleoptiPrincipal.Current.Regional.UICulture;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromTime">A start-time reference.</param>
        /// <param name="toTime">An end-time reference.</param>
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