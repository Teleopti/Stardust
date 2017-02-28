using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used to set new skill staff period values to a skill day.
    /// </summary>
    public interface INewSkillStaffPeriodValues
    {
        /// <summary>
        /// Sets the action to perform when batch completed is called.
        /// </summary>
        /// <param name="calculateChildSkillDay">The action to perform.</param>
        void RunWhenBatchCompleted(Action<object> calculateChildSkillDay);
        
        /// <summary>
        /// Sets the target skill staff periods.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods that should be updated with values from the contained skill staff periods.</param>
        void SetValues(IEnumerable<ISkillStaffPeriod> skillStaffPeriods);

        /// <summary>
        /// Indicates that the batch was completed.
        /// </summary>
        void BatchCompleted();
    }
}