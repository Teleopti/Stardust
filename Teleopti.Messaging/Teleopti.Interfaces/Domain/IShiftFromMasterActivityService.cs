﻿using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for ShiftFromMasterActivityService
    /// </summary>
    public interface IShiftFromMasterActivityService
    {
        /// <summary>
        /// Generates workshifts based on master activities
        /// </summary>
        /// <param name="workShift"></param>
        /// <returns></returns>
        IList<IWorkShift> ExpandWorkShiftsWithMasterActivity(IWorkShift workShift);
    }
}
