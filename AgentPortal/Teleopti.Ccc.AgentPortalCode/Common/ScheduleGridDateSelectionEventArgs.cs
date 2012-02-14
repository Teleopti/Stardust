using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    /// <summary>
    /// ScheduleGridDateSelectionEventArgs
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 2008-11-10
    /// </remarks>
    public class ScheduleGridDateSelectionEventArgs : EventArgs
    {

        private readonly IList<DateTime> _selectedDateList = null;

        /// <summary>
        /// Gets the dates.
        /// </summary>
        /// <value>The dates.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-10
        /// </remarks>
        public ReadOnlyCollection<DateTime> Dates
        {
            get
            {
                return new ReadOnlyCollection<DateTime>(_selectedDateList);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleGridDateSelectionEventArgs"/> class.
        /// </summary>
        /// <param name="selectedDateList">The selected date list.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-10
        /// </remarks>
        public ScheduleGridDateSelectionEventArgs(IList<DateTime> selectedDateList)
        {
            _selectedDateList = selectedDateList;
        }
    }
}
