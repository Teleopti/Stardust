using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for matrix value calculator. Calculates the value of the selected schedule matrix.
    /// </summary>
    public interface IScheduleMatrixValueCalculatorPro : IPeriodValueCalculator
    {
        /// <summary>
        /// The value calculated for the skills in the list.
        /// </summary>
        /// <param name="scheduleDay">The schedule day.</param>
        /// <param name="skillList">The skill list.</param>
        /// <returns></returns>
        double? DayValueForSkills(DateOnly scheduleDay, IList<ISkill> skillList);
    }
}