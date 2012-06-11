using System;
using System.Collections.Generic;
using System.Text;
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
        DateTime StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DateTime EndTime { get; set; }
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
