using System;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    /// <summary>
    /// Represents MultiplicatorTypeView.
    /// </summary>
    public class DayOfWeekAdapter
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-10
        /// </remarks>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-10
        /// </remarks>
        public DayOfWeek DayOfWeek { get; set; }

        public DayOfWeekAdapter(DayOfWeek dayOfWeek)
        {
            DisplayName = LanguageResourceHelper.TranslateEnumValue(dayOfWeek);
            DayOfWeek = dayOfWeek;
        }
    }
}
