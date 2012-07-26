using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPossibleStartEndCategory
    {
        /// <summary>
        /// 
        /// </summary>
        TimeSpan StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
		TimeSpan EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        IShiftCategory ShiftCategory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        double ShiftValue { get; set; }
    }
}
