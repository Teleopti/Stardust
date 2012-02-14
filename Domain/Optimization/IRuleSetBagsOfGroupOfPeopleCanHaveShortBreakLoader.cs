using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Loads and creates classes for IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak
    /// </summary>
    public interface IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader
    {
        /// <summary>
        /// Creates the work rule set extractor.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <returns></returns>
        IWorkRuleSetExtractorForGroupOfPeople CreateWorkRuleSetExtractor(IEnumerable<IPerson> persons);

        /// <summary>
        /// Creates the work shift rule set has short break.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <returns></returns>
        IWorkShiftRuleSetCanHaveShortBreak CreateWorkShiftRuleSetCanHaveShortBreak(IEnumerable<IPerson> persons);
    }
}