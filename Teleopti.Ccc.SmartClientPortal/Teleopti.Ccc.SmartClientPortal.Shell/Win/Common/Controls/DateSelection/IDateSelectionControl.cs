using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
{
    public interface IDateSelectionControl
    {
        /// <summary>
        /// Occurs when [date range changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        event EventHandler<DateRangeChangedEventArgs> DateRangeChanged;

        /// <summary>
        /// Gets or sets a value indicating whether [show apply button].
        /// </summary>
        /// <value><c>true</c> if [show apply button]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(true), DefaultValue(true), Category("Teleopti Behavior")]
        bool ShowApplyButton { get; set; }

        /// <summary>
        /// Gets the selected dates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-02
        /// </remarks>
        [Browsable(false)]
        IList<DateOnlyPeriod> GetSelectedDates();
    }
}