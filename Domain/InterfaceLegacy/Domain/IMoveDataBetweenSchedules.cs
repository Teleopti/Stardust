using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;


namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_HideExportSchedule_81161)]
	public interface IMoveDataBetweenSchedules
    {
        /// <summary>
        /// Exports the specified persons and period.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="sourceParts">The source parts.</param>
        /// <returns>
        /// Returns the business rules that this method has overriden.
        /// Use it to show a warning of what has happened.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-10-12
        /// </remarks>
        IEnumerable<IBusinessRuleResponse> CopySchedulePartsToAnotherDictionary(IScheduleDictionary destination, IEnumerable<IScheduleDay> sourceParts);
    }

}
