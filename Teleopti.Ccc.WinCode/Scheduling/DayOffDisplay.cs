using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Describes how an PersonDayOff should be displayed in period view
    /// Zoë
    /// </summary>
    public class DayOffDisplay
    {
        private readonly PersonDayOff _dayOff;
        private readonly DisplayMode _displayMode;

        /// <summary>
        /// Creates an instance of DayOffDisplay
        /// </summary>
        /// <param name="dayOff"></param>
        public DayOffDisplay(PersonDayOff dayOff)
        {
            _dayOff = dayOff;
            _displayMode = DisplayMode.DayOff;
        }

        /// <summary>
        /// Gets the DayOff
        /// </summary>
        public PersonDayOff DayOff
        {
            get { return _dayOff; }
        }

        /// <summary>
        /// Gets the Anchordate
        /// </summary>
        public DateTime AnchorDate
        {
            get { return _dayOff.DayOff.AnchorLocal.Date; }
        }

        /// <summary>
        /// Gets the DisplayMode
        /// </summary>
        public DisplayMode DisplayMode
        {
            get { return _displayMode; }
        }
    }
}