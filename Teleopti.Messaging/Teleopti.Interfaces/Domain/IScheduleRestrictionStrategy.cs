using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    public interface IScheduleRestrictionStrategy
    {
        /// <summary>
        /// Extracts the restrictions
        /// some probable refactoring to pass on team block to get the restriction
        /// </summary>
        /// <param name="dateOnlyList"></param>
        /// <param name="matrixList"></param>
        /// <returns></returns>
        IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList);
    }
}
