using System;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Monitors visualtimezone.
    /// If the visualtimezoninfo changes, all DataContext that implements IVisualTimeZoneInfoMonitor
    /// will get called with the new timezone
    /// </summary>
    /// <remarks>
    /// If you use converters that already uses IVisualTimeZonInfo, there is no need to handle the change in the model,
    /// just keep everything as UTC
    /// Created by: henrika
    /// Created date: 2009-08-25
    /// </remarks>
    public interface IVisualTimeZoneInfoMonitor
    {
        /// <summary>
        /// Times the zone info changed.
        /// </summary>
        /// <param name="newTimeZoneInfo">The .</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-08-25
        /// </remarks>
        void TimeZoneInfoChanged(TimeZoneInfo newTimeZoneInfo);
    }
}
