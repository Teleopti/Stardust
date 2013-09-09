using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// OptimizationRestriction
    /// </summary>
    public enum OptimizationRestriction
    {

        /// <summary>
        /// Do not have to keep anything
        /// </summary>
        None, 

        /// <summary>
        /// Keep same shift category
        /// </summary>
        KeepShiftCategory,

        /// <summary>
        /// Keep same start and end time
        /// </summary>
        KeepStartAndEndTime
    }
}
